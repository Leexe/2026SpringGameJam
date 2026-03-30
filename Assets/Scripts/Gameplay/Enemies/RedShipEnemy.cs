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
	private bool _enableShooting;

	private void Start()
	{
		StopShooting();
	}

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGameStart.AddListener(StopShooting);
			GameManager.Instance.OnFadeOutFinish.AddListener(StopShooting);
			GameManager.Instance.OnGameWin.AddListener(StopShooting);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGameStart.RemoveListener(StopShooting);
			GameManager.Instance.OnFadeOutFinish.RemoveListener(StopShooting);
			GameManager.Instance.OnGameWin.RemoveListener(StopShooting);
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

	/* Public Methods */

	public void StartShooting()
	{
		_enableShooting = true;
	}

	public void StopShooting()
	{
		_enableShooting = false;
	}
}
