using Sirenix.OdinInspector;
using UnityEngine;

public class AsteroidAttack : MonoBehaviour
{
	[Header("Attack Settings")]
	[SerializeField, Required]
	[Tooltip("Ensure this AttackSO has Direction set to 270 (down) and TowardsPlayer set to false!")]
	private AttackSO _asteroidPattern;

	[SerializeField]
	private float _asteroidsPerSecond = 5f;

	[SerializeField]
	private float _minSpeed = 5f;

	[SerializeField]
	private float _maxSpeed = 10f;

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
	private bool _isAttacking;

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

		// 3. Fire using the BulletManager with our random speed override
		BulletManager.Instance.FireAttack(_asteroidPattern, spawnPosition, speed);
	}
}
