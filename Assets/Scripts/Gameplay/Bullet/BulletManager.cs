using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
	#region Properties

	public static BulletManager Instance { get; private set; }

	#endregion

	#region Fields

	[Header("Memory Settings")]
	[Tooltip("Pre-allocate this many bullets. Zero runtime allocations. (Keep under 100k for WebGL)")]
	[SerializeField]
	private int _maxBullets = 5000;

	[Header("Gameplay (Testing)")]
	[SerializeField]
	private Transform _playerTransform;

	[SerializeField]
	private float _playerHurtboxRadius = 0.2f;

	// Core DOD Data
	private Bullet[] _bullets;
	private Stack<int> _freeIndices;

	// Batched Rendering Data
	private readonly Dictionary<BulletData, List<Matrix4x4>> _renderBatches = new();
	private readonly Matrix4x4[] _drawBuffer = new Matrix4x4[1023]; // Unity's limit per draw call
	#endregion

	#region Lifecycle

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		InitializePool();
	}

	private void Update()
	{
		// 1. Prepare render batches memory
		foreach (KeyValuePair<BulletData, List<Matrix4x4>> kvp in _renderBatches)
		{
			kvp.Value.Clear();
		}

		Vector2 playerPos = _playerTransform ? _playerTransform.position : Vector2.zero;
		float playerRadiusSqr = _playerHurtboxRadius * _playerHurtboxRadius;

		// 2. The Core Update Loop
		// This is blindingly fast because it's a tight loop over a contiguous struct array
		for (int i = 0; i < _bullets.Length; i++)
		{
			if (!_bullets[i].IsActive)
			{
				continue;
			}

			_bullets[i].TimeAlive += Time.deltaTime;

			// ---- BEHAVIOR SYSTEM ----
			switch (_bullets[i].Behavior)
			{
				case BulletBehavior.Linear:
					_bullets[i].Position += _bullets[i].Velocity * Time.deltaTime;
					break;
				case BulletBehavior.SineWave:
					// Find the right perpendicular vector, apply sine offset
					Vector2 forwardDir = _bullets[i].Velocity.normalized;
					var rightDir = new Vector2(-forwardDir.y, forwardDir.x);
					float offset = Mathf.Cos(_bullets[i].TimeAlive * _bullets[i].Frequency) * _bullets[i].Amplitude;
					_bullets[i].Position +=
						(_bullets[i].Velocity * Time.deltaTime) + (rightDir * (offset * Time.deltaTime));
					break;
				case BulletBehavior.Tracking:
					if (_playerTransform != null)
					{
						Vector2 dirToTarget = (playerPos - _bullets[i].Position).normalized;
						_bullets[i].Velocity = Vector2.Lerp(
							_bullets[i].Velocity,
							dirToTarget * _bullets[i].Velocity.magnitude,
							Time.deltaTime * 3f
						);
						_bullets[i].Position += _bullets[i].Velocity * Time.deltaTime;
					}
					break;
			}

			// ---- COLLISION SYSTEM ----
			if (_playerTransform != null)
			{
				float hitRadiusSqr = playerRadiusSqr + (_bullets[i].Data.HitboxRadius * _bullets[i].Data.HitboxRadius);
				if ((_bullets[i].Position - playerPos).sqrMagnitude < hitRadiusSqr)
				{
					Debug.Log("Player Hit by Bullet!");
					KillBullet(i);
					continue; // Dead, so don't render it
				}
			}

			// Off-screen cleanup bounds (-30 to +30 units camera view)
			if (Mathf.Abs(_bullets[i].Position.x) > 30f || Mathf.Abs(_bullets[i].Position.y) > 30f)
			{
				KillBullet(i);
				continue;
			}

			// ---- RENDERING QUEUE ----
			// TRS (Translation, Rotation, Scale). We don't rotate basic bullet sprites usually for optimization
			var matrix = Matrix4x4.TRS(
				_bullets[i].Position,
				Quaternion.identity,
				Vector3.one * _bullets[i].Data.VisualScale
			);
			_renderBatches[_bullets[i].Data].Add(matrix);
		}

		// 3. Batched DrawMeshInstanced execution
		foreach (KeyValuePair<BulletData, List<Matrix4x4>> kvp in _renderBatches)
		{
			BulletData data = kvp.Key;
			List<Matrix4x4> matrices = kvp.Value;

			if (!data || !data.Mesh || !data.Material || matrices.Count == 0)
			{
				continue;
			}

			// Chunk into arrays of 1023
			int totalCount = matrices.Count;
			for (int i = 0; i < totalCount; i += 1023)
			{
				int length = Mathf.Min(1023, totalCount - i);
				matrices.CopyTo(i, _drawBuffer, 0, length);

				// The massive GPU speedup happens right here
				Graphics.DrawMeshInstanced(data.Mesh, 0, data.Material, _drawBuffer, length);
			}
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Reads a PatternData config and spawns the exact amount of bullets across the spread angle.
	/// </summary>
	public void FirePattern(PatternData pattern, Vector2 origin, Vector2 aimDirection)
	{
		if (pattern.BulletType == null)
		{
			Debug.LogWarning("Pattern has no BulletData assigned!");
			return;
		}

		// Ensure we have a rendering bucket ready for this bullet's visual material
		if (!_renderBatches.ContainsKey(pattern.BulletType))
		{
			_renderBatches[pattern.BulletType] = new List<Matrix4x4>(_maxBullets);
		}

		// Convert direction Vector to Radian rotation
		float startAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

		// Calculate slice per bullet
		float angleStep = pattern.BulletCount > 1 ? pattern.SpreadAngle / (pattern.BulletCount - 1) : 0;
		float currentAngle = startAngle - (pattern.SpreadAngle / 2f);

		for (int i = 0; i < pattern.BulletCount; i++)
		{
			if (_freeIndices.Count == 0)
			{
				Debug.LogWarning("Bullet pool exhausted! Increase Max Bullets.");
				return;
			}

			// Math to get XY vector from angle
			float rad = currentAngle * Mathf.Deg2Rad;
			Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * pattern.BaseSpeed;

			// O(1) Pop
			int index = _freeIndices.Pop();

			// Assign struct properties instantly
			_bullets[index].IsActive = true;
			_bullets[index].Data = pattern.BulletType;
			_bullets[index].Position = origin;
			_bullets[index].Velocity = velocity;
			_bullets[index].Behavior = pattern.Behavior;
			_bullets[index].TimeAlive = 0f;
			_bullets[index].Amplitude = pattern.SineAmplitude;
			_bullets[index].Frequency = pattern.SineFrequency;

			currentAngle += angleStep;
		}
	}

	#endregion

	#region Private Methods

	private void InitializePool()
	{
		// O(1) Allocations. This happens exactly once on awake.
		_bullets = new Bullet[_maxBullets];
		_freeIndices = new Stack<int>(_maxBullets);

		// Push all array indices into the "availability" pool
		for (int i = _maxBullets - 1; i >= 0; i--)
		{
			_freeIndices.Push(i);
		}
	}

	private void KillBullet(int index)
	{
		_bullets[index].IsActive = false;
		// O(1) Return to pool
		_freeIndices.Push(index);
	}

	#endregion
}
