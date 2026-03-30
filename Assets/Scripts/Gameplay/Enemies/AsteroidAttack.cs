using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class AsteroidAttack : MonoBehaviour
{
	[Header("Attack Settings")]
	[SerializeField, Required]
	[Tooltip("Ensure this AttackSO has Direction set to 270 (down) and TowardsPlayer set to false!")]
	private AttackSO _asteroidPattern;

	[SerializeField]
	private float _asteroidsPerSecondEasy = 5f;

	[SerializeField]
	private float _asteroidsPerSecondHard = 10f;

	[SerializeField]
	private float _minSpeedEasy = 5f;

	[SerializeField]
	private float _maxSpeedEasy = 10f;

	[SerializeField]
	private float _minSpeedHard = 5f;

	[SerializeField]
	private float _maxSpeedHard = 10f;

	[SerializeField, Range(0f, 1f)]
	private float _intensity = 0f;

	[SerializeField]
	private bool _isAttacking;

	public float Intensity
	{
		get => _intensity;
		set => _intensity = Mathf.Clamp01(value);
	}

	private float _asteroidsPerSecond => Mathf.Lerp(_asteroidsPerSecondEasy, _asteroidsPerSecondHard, _intensity);
	private float _minSpeed => Mathf.Lerp(_minSpeedEasy, _minSpeedHard, _intensity);
	private float _maxSpeed => Mathf.Lerp(_maxSpeedEasy, _maxSpeedHard, _intensity);

	[Header("Spawn Layout")]
	[SerializeField, Tooltip("The Y coordinate above the camera to spawn asteroids")]
	private float _spawnYPlane = 15f;

	[SerializeField, Tooltip("The horizontal range from the center to spawn asteroids")]
	private float _spawnXRange = 25f;

	[Header("Determinism")]
	[SerializeField, Tooltip("The seed used to guarantee the exact same asteroid patterns every run.")]
	private int _seed = 2034;

	private System.Random _rng;
	private float _spawnTimer;

	private void Awake()
	{
		ResetPattern();
	}

	private void OnEnable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnGameRestart.AddListener(ResetPattern);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnGameRestart.RemoveListener(ResetPattern);
		}
	}

	[Button]
	public void StartAttack()
	{
		_isAttacking = true;
	}

	[Button]
	public void StopAttack()
	{
		_isAttacking = false;
	}

	public void ResetPattern()
	{
		// Initialize with fixed seed so the sequence of asteroids is perfectly deterministic
		_rng = new System.Random(_seed);
		_spawnTimer = 0f;
		StopAttack();
	}

	private void Update()
	{
		if (!_isAttacking || _asteroidPattern == null)
		{
			return;
		}

		if (_asteroidsPerSecond <= 0)
		{
			return;
		}

		float interval = 1f / _asteroidsPerSecond;
		_spawnTimer += Time.deltaTime;

		while (_spawnTimer >= interval)
		{
			_spawnTimer -= interval;
			SpawnAsteroid();
		}
	}

	private void SpawnAsteroid()
	{
		// 1. Calculate deterministic random X position
		float normalizedX = (float)_rng.NextDouble(); // Returns 0.0 to 1.0
		float spawnX = Mathf.Lerp(-_spawnXRange, _spawnXRange, normalizedX);
		Vector2 spawnPosition = new Vector2(spawnX, _spawnYPlane);

		// 2. Calculate deterministic random speed
		float normalizedSpeed = (float)_rng.NextDouble();
		float speed = Mathf.Lerp(_minSpeed, _maxSpeed, normalizedSpeed);

		// 3. Calculate deterministic random direction based on the spread angle
		float spreadAngle = _asteroidPattern != null ? _asteroidPattern.SpreadAngle : 0f;
		float baseDirection = _asteroidPattern != null ? _asteroidPattern.Direction : 270f;
		float normalizedAngle = (float)_rng.NextDouble();
		float randomDirection = baseDirection - (spreadAngle / 2f) + (spreadAngle * normalizedAngle);

		// 4. Calculate deterministic random visual rotation
		float randomRotation = (float)(_rng.NextDouble() * 360f);

		// 5. Fire using the BulletManager with overrides
		BulletManager.Instance.FireAttack(_asteroidPattern, spawnPosition, speed, randomDirection, randomRotation);
	}
}
