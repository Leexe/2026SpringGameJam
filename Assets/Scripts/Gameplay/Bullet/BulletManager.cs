using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class BulletManager : MonoSingleton<BulletManager>
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

	[Header("Debug")]
	[SerializeField]
	private bool _drawHitboxes = false;

	// Events
	public event Action OnPlayerCollision;

	// Bullets Pool
	private Bullet[] _bullets;
	private Stack<int> _freeInstances;

	// Batched Rendering Data
	private readonly Dictionary<BulletSO, List<Matrix4x4>> _renderBatches = new();
	private readonly Dictionary<BulletSO, List<float>> _alphaBatches = new();

	// DrawMeshInstanced can only draw 1023 matrices per draw call, so we have to chunk them
	private readonly Matrix4x4[] _drawBuffer = new Matrix4x4[1023];
	private readonly float[] _alphaBuffer = new float[1023];
	private MaterialPropertyBlock _propertyBlock;
	private static readonly int AlphaPropertyId = Shader.PropertyToID("_InstanceAlpha");

	#endregion

	#region Lifecycle

	protected override void Awake()
	{
		base.Awake();

		InitializePool();
		_propertyBlock = new MaterialPropertyBlock();
	}

	private void Update()
	{
		// 1. Flush Memory
		foreach (KeyValuePair<BulletSO, List<Matrix4x4>> kvp in _renderBatches)
		{
			kvp.Value.Clear();
		}
		foreach (KeyValuePair<BulletSO, List<float>> kvp in _alphaBatches)
		{
			kvp.Value.Clear();
		}

		Vector2 playerPos = _playerTransform ? _playerTransform.position : Vector2.zero;

		// 2. Core Update Loop, Loop through all the bullets
		for (int i = 0; i < _bullets.Length; i++)
		{
			ref Bullet bullet = ref _bullets[i];

			if (!bullet.IsActive)
			{
				continue;
			}

			bullet.TimeAlive += Time.deltaTime;

			if (bullet.IsFading)
			{
				bullet.FadeTimer += Time.deltaTime;
				if (bullet.FadeTimer >= bullet.FadeDuration)
				{
					KillBullet(i);
					continue;
				}
			}

			if (!bullet.IsFading && bullet.TimeAlive >= bullet.MaxLifeTime)
			{
				DeactivateBullet(i);
				continue;
			}

			// Behaviors System
			switch (bullet.Behavior)
			{
				case BulletBehavior.Linear:
					bullet.Position += bullet.Velocity * Time.deltaTime;
					break;
				case BulletBehavior.SineWave:
					Vector2 forwardDir = bullet.Velocity.normalized;
					var rightDir = new Vector2(-forwardDir.y, forwardDir.x);
					float offset = Mathf.Cos(bullet.TimeAlive * bullet.Frequency) * bullet.Amplitude;
					bullet.Position += (bullet.Velocity * Time.deltaTime) + (rightDir * (offset * Time.deltaTime));
					break;
				case BulletBehavior.Following:
					if (_playerTransform)
					{
						Vector2 targetDir = (playerPos - bullet.Position).normalized;
						bullet.Velocity = Vector2.Lerp(
							bullet.Velocity,
							targetDir * bullet.Speed,
							Time.deltaTime * bullet.TrackingStrength
						);
					}
					bullet.Position += bullet.Velocity * Time.deltaTime;
					break;
				case BulletBehavior.Steering:
					Vector2 velToTarget = (playerPos - bullet.Position).normalized * bullet.Speed;
					Vector2 steeringForce = velToTarget - bullet.Velocity;
					steeringForce = Vector2.ClampMagnitude(steeringForce, bullet.MaxSteerForce * bullet.Speed);
					bullet.Velocity += steeringForce * Time.deltaTime;
					bullet.Velocity = Vector2.ClampMagnitude(bullet.Velocity, bullet.Speed);
					bullet.Position += bullet.Velocity * Time.deltaTime;
					break;
				case BulletBehavior.Homing:
					Vector2 dirToTarget = playerPos - bullet.Position;
					float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
					float angleDiff = Mathf.DeltaAngle(bullet.Heading * Mathf.Rad2Deg, targetAngle);

					bullet.AngularVelocity += Mathf.Sign(angleDiff) * bullet.TurnAcceleration * Time.deltaTime;
					bullet.AngularVelocity = Mathf.Clamp(
						bullet.AngularVelocity,
						-bullet.MaxTurnRate,
						bullet.MaxTurnRate
					);

					bullet.Heading += bullet.AngularVelocity * Mathf.Deg2Rad * Time.deltaTime;
					bullet.Velocity = new Vector2(Mathf.Cos(bullet.Heading), Mathf.Sin(bullet.Heading)) * bullet.Speed;
					bullet.Position += bullet.Velocity * Time.deltaTime;
					break;
			}

			// Collisions System
			if (!bullet.IsFading && _playerTransform)
			{
				float combinedRadius = _playerHurtboxRadius + bullet.HitRadius;
				float hitRadiusSqr = combinedRadius * combinedRadius;
				if ((bullet.Position - playerPos).sqrMagnitude < hitRadiusSqr)
				{
					DeactivateBullet(i);
					OnPlayerCollision?.Invoke();
					continue;
				}
			}

			// Off-screen cleanup bounds
			if (Mathf.Abs(bullet.Position.x) > _bulletBounds || Mathf.Abs(bullet.Position.y) > _bulletBounds)
			{
				KillBullet(i);
				continue;
			}

			// Prepare for Rendering
			float alpha = bullet.IsFading ? 1f - (bullet.FadeTimer / bullet.FadeDuration) : 1f;
			AddToRenderBatch(ref bullet, alpha);
		}

		// 3. Render with DrawMeshInstanced
		foreach (KeyValuePair<BulletSO, List<Matrix4x4>> kvp in _renderBatches)
		{
			BulletSO so = kvp.Key;
			List<Matrix4x4> matrices = kvp.Value;
			List<float> alphas = _alphaBatches[so];

			if (!so || !so.Mesh || !so.Material || matrices.Count == 0)
			{
				continue;
			}

			int totalCount = matrices.Count;
			for (int i = 0; i < totalCount; i += 1023)
			{
				int length = Mathf.Min(1023, totalCount - i);
				matrices.CopyTo(i, _drawBuffer, 0, length);
				alphas.CopyTo(i, _alphaBuffer, 0, length);

				_propertyBlock.SetFloatArray(AlphaPropertyId, _alphaBuffer);
				Graphics.DrawMeshInstanced(so.Mesh, 0, so.Material, _drawBuffer, length, _propertyBlock);
			}
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Reads PatternSO and spawns bullets
	/// </summary>
	[FoldoutGroup("Debug")]
	[Button]
	public void FirePattern(PatternSO pattern, Vector2 origin)
	{
		if (!pattern.BulletSO)
		{
			Debug.LogWarning("Pattern has no BulletData assigned!");
			return;
		}

		// Create a new bucket if not already inside dictionary
		if (!_renderBatches.ContainsKey(pattern.BulletSO))
		{
			_renderBatches[pattern.BulletSO] = new List<Matrix4x4>(_maxBullets);
			_alphaBatches[pattern.BulletSO] = new List<float>(_maxBullets);
		}

		float startAngle = pattern.Direction;
		if (pattern.TowardsPlayer && _playerTransform)
		{
			Vector2 dirToPlayer = (Vector2)_playerTransform.position - origin;
			startAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
		}

		// Calculate slice per bullet
		float angleStep = pattern.BulletCount > 1 ? pattern.SpreadAngle / (pattern.BulletCount - 1) : 0;
		float currentAngle = pattern.BulletCount > 1 ? startAngle - (pattern.SpreadAngle / 2f) : startAngle;

		for (int i = 0; i < pattern.BulletCount; i++)
		{
			if (_freeInstances.Count == 0)
			{
				Debug.LogWarning("Bullet pool exhausted, Increase Max Bullets");
				return;
			}

			float rad = currentAngle * Mathf.Deg2Rad;
			Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * pattern.BaseSpeed;

			int index = _freeInstances.Pop();

			_bullets[index].IsActive = true;
			_bullets[index].SO = pattern.BulletSO;
			_bullets[index].Position = origin;
			_bullets[index].Velocity = velocity;
			_bullets[index].Speed = pattern.BaseSpeed;
			_bullets[index].RotateTowardsDirection = pattern.RotateTowardsDirection;
			_bullets[index].HitRadius = pattern.BulletSO.HitboxRadius;
			_bullets[index].Behavior = pattern.Behavior;
			_bullets[index].TimeAlive = 0f;
			_bullets[index].MaxLifeTime = pattern.MaxLifeTime;
			_bullets[index].IsFading = false;
			_bullets[index].FadeTimer = 0f;
			_bullets[index].FadeDuration = pattern.FadeDuration;
			_bullets[index].Amplitude = pattern.SineAmplitude;
			_bullets[index].Frequency = pattern.SineFrequency;
			_bullets[index].TrackingStrength = pattern.TrackingStrength;
			_bullets[index].MaxSteerForce = pattern.MaxSteerForce;

			// Homing parameters
			_bullets[index].Heading = rad;
			_bullets[index].AngularVelocity = 0f;
			_bullets[index].MaxTurnRate = pattern.MaxTurnRate;
			_bullets[index].TurnAcceleration = pattern.TurnAcceleration;

			currentAngle += angleStep;
		}
	}

	#endregion

	#region Private Methods

	private void InitializePool()
	{
		_bullets = new Bullet[_maxBullets];
		_freeInstances = new Stack<int>(_maxBullets);

		for (int i = _maxBullets - 1; i >= 0; i--)
		{
			_freeInstances.Push(i);
		}
	}

	private void DeactivateBullet(int index)
	{
		_bullets[index].IsFading = true;
		_bullets[index].FadeTimer = 0f;
	}

	private void KillBullet(int index)
	{
		_bullets[index].IsActive = false;
		_bullets[index].IsFading = false;
		_freeInstances.Push(index);
	}

	private void AddToRenderBatch(ref Bullet bullet, float alpha)
	{
		Quaternion targetRotation = Quaternion.identity;
		if (bullet.RotateTowardsDirection)
		{
			float angle = Mathf.Atan2(bullet.Velocity.y, bullet.Velocity.x) * Mathf.Rad2Deg;
			targetRotation = Quaternion.Euler(0, 0, angle);
		}

		var matrix = Matrix4x4.TRS(bullet.Position, targetRotation, Vector3.one * bullet.SO.VisualScale);
		_renderBatches[bullet.SO].Add(matrix);
		_alphaBatches[bullet.SO].Add(alpha);
	}

	private void OnDrawGizmos()
	{
		if (!_drawHitboxes || _bullets == null)
		{
			return;
		}

		// Draw player hurtbox
		if (_playerTransform)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(_playerTransform.position, _playerHurtboxRadius);
		}

		// Draw bullet hitboxes
		Gizmos.color = Color.red;
		for (int i = 0; i < _bullets.Length; i++)
		{
			if (!_bullets[i].IsActive)
			{
				continue;
			}

			Gizmos.DrawWireSphere(_bullets[i].Position, _bullets[i].HitRadius);
		}
	}

	#endregion
}
