using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
	#region Fields

	[Header("Memory Settings")]
	[Tooltip("Number of Pre-Allocated bullets.")]
	[SerializeField]
	private int _maxBullets = 5000;

	[Header("Gameplay")]
	[SerializeField]
	private Transform _playerTransform;

	[SerializeField]
	private float _playerHurtboxRadius = 0.25f;

	[SerializeField]
	[Tooltip("Size of square around origin to delete bullets")]
	private float _bulletBounds = 30f;

	private float _playerRadiusSqr;

	// Bullets Pool
	private Bullet[] _bullets;
	private Stack<int> _freeIndices;

	// Batched Rendering Data
	private readonly Dictionary<BulletSO, List<Matrix4x4>> _renderBatches = new();

	// DrawMeshInstanced can only draw 1023 matrices per draw call, so we have to chunk them
	private readonly Matrix4x4[] _drawBuffer = new Matrix4x4[1023];

	public static BulletManager Instance { get; private set; }

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
		_playerRadiusSqr = _playerHurtboxRadius * _playerHurtboxRadius;
	}

	private void Update()
	{
		// 1. Flush Memory
		foreach (KeyValuePair<BulletSO, List<Matrix4x4>> kvp in _renderBatches)
		{
			kvp.Value.Clear();
		}

		Vector2 playerPos = _playerTransform ? _playerTransform.position : Vector2.zero;

		// 2. Core Update Loop, Loop through all the bullets
		for (int i = 0; i < _bullets.Length; i++)
		{
			if (!_bullets[i].IsActive)
			{
				continue;
			}

			_bullets[i].TimeAlive += Time.deltaTime;

			// Behaviors System
			switch (_bullets[i].Behavior)
			{
				case BulletBehavior.Linear:
					_bullets[i].Position += _bullets[i].Velocity * Time.deltaTime;
					break;
				case BulletBehavior.SineWave:
					Vector2 forwardDir = _bullets[i].Velocity.normalized;
					var rightDir = new Vector2(-forwardDir.y, forwardDir.x);
					float offset = Mathf.Cos(_bullets[i].TimeAlive * _bullets[i].Frequency) * _bullets[i].Amplitude;
					_bullets[i].Position +=
						(_bullets[i].Velocity * Time.deltaTime) + (rightDir * (offset * Time.deltaTime));
					break;
				case BulletBehavior.Tracking:
					if (_playerTransform)
					{
						Vector2 dirToTarget = (playerPos - _bullets[i].Position).normalized;
						_bullets[i].Velocity = Vector2.Lerp(
							_bullets[i].Velocity,
							dirToTarget * _bullets[i].Speed,
							Time.deltaTime * _bullets[i].TrackingStrength
						);
						_bullets[i].Position += _bullets[i].Velocity * Time.deltaTime;
					}
					break;
			}

			// Collisions System
			if (_playerTransform)
			{
				float hitRadiusSqr = _playerRadiusSqr + _bullets[i].HitRadiusSqr;
				if ((_bullets[i].Position - playerPos).sqrMagnitude < hitRadiusSqr)
				{
					Debug.Log("Player Hit by Bullet");
					KillBullet(i);
					continue;
				}
			}

			// Off-screen cleanup bounds
			if (Mathf.Abs(_bullets[i].Position.x) > _bulletBounds || Mathf.Abs(_bullets[i].Position.y) > _bulletBounds)
			{
				KillBullet(i);
				continue;
			}

			// Prepare for Rendering
			Quaternion targetRotation = _bullets[i].RotateTowardsDirection
				? Quaternion.LookRotation(_bullets[i].Velocity)
				: Quaternion.identity;

			var matrix = Matrix4x4.TRS(_bullets[i].Position, targetRotation, Vector3.one * _bullets[i].SO.VisualScale);
			_renderBatches[_bullets[i].SO].Add(matrix);
		}

		// 3. Render with DrawMeshInstanced
		foreach (KeyValuePair<BulletSO, List<Matrix4x4>> kvp in _renderBatches)
		{
			BulletSO so = kvp.Key;
			List<Matrix4x4> matrices = kvp.Value;

			if (!so || !so.Mesh || !so.Material || matrices.Count == 0)
			{
				continue;
			}

			int totalCount = matrices.Count;
			for (int i = 0; i < totalCount; i += 1023)
			{
				int length = Mathf.Min(1023, totalCount - i);
				matrices.CopyTo(i, _drawBuffer, 0, length);

				Graphics.DrawMeshInstanced(so.Mesh, 0, so.Material, _drawBuffer, length);
			}
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Reads a PatternData config and spawns the exact amount of bullets across the spread angle.
	/// </summary>
	public void FirePattern(PatternSO pattern, Vector2 origin, Vector2 aimDirection)
	{
		if (pattern.BulletSO == null)
		{
			Debug.LogWarning("Pattern has no BulletData assigned!");
			return;
		}

		// Ensure we have a rendering bucket ready for this bullet's visual material
		if (!_renderBatches.ContainsKey(pattern.BulletSO))
		{
			_renderBatches[pattern.BulletSO] = new List<Matrix4x4>(_maxBullets);
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
			_bullets[index].SO = pattern.BulletSO;
			_bullets[index].Position = origin;
			_bullets[index].Velocity = velocity;
			_bullets[index].Speed = pattern.BaseSpeed;
			_bullets[index].RotateTowardsDirection = pattern.RotateTowardsDirection;
			_bullets[index].HitRadiusSqr = pattern.BulletSO.HitboxRadius * pattern.BulletSO.HitboxRadius;
			_bullets[index].Behavior = pattern.Behavior;
			_bullets[index].TimeAlive = 0f;
			_bullets[index].Amplitude = pattern.SineAmplitude;
			_bullets[index].Frequency = pattern.SineFrequency;
			_bullets[index].TrackingStrength = pattern.TrackingStrength;

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
