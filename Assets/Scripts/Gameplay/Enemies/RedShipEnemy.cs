using System.Collections.Generic;
using UnityEngine;

public class RedShipEnemy : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private List<PatternSO> _patterns;

	[SerializeField]
	private List<Transform> _shootPoints;

	[Header("Enemy Settings")]
	[SerializeField]
	[Tooltip("How long it takes to shoot a shot in seconds")]
	private float _fireRate = 1f;

	private float _fireRateTimer;
	private int _patternIndex;
	private int _shootPointIndex;
	private bool _enableShooting = false;

	private void Awake()
	{
		DisableEnemy();
	}

	private void Start()
	{
		DisableEnemy();
	}

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGameStart.AddListener(EnableEnemy);
			GameManager.Instance.OnFadeOutFinish.AddListener(DisableEnemy);
			GameManager.Instance.OnGameWin.AddListener(DisableEnemy);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGameStart.RemoveListener(EnableEnemy);
			GameManager.Instance.OnFadeOutFinish.RemoveListener(DisableEnemy);
			GameManager.Instance.OnGameWin.RemoveListener(DisableEnemy);
		}
	}

	private void Update()
	{
		if (_enableShooting)
		{
			if (_fireRateTimer >= _fireRate)
			{
				BulletManager.Instance.FirePattern(_patterns[_patternIndex], _shootPoints[_shootPointIndex].position);
				_patternIndex = (_patternIndex + 1) % _patterns.Count;
				_shootPointIndex = (_shootPointIndex + 1) % _shootPoints.Count;
				_fireRateTimer -= _fireRate;
			}
			_fireRateTimer += Time.deltaTime;
		}
	}

	private void EnableEnemy()
	{
		_enableShooting = true;
		_spriteRenderer.enabled = true;
	}

	private void DisableEnemy()
	{
		_enableShooting = false;
		_spriteRenderer.enabled = false;
	}
}
