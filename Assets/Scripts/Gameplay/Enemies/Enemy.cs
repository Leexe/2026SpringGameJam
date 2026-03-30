using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private List<PatternSO> _patterns;

	[SerializeField]
	private List<Transform> _shootPoints;

	private int _patternIndex;
	private int _attackIndex;
	private int _shootPointIndex;
	private bool _enableShooting;
	private float _delayTimer;
	private bool _isWaitingForDelay;

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
			GameManager.Instance.OnPlayerDeath.AddListener(StopShooting);
			GameManager.Instance.OnGameRestart.AddListener(StopShooting);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGameStart.RemoveListener(StopShooting);
			GameManager.Instance.OnFadeOutFinish.RemoveListener(StopShooting);
			GameManager.Instance.OnGameWin.RemoveListener(StopShooting);
			GameManager.Instance.OnPlayerDeath.RemoveListener(StopShooting);
			GameManager.Instance.OnGameRestart.RemoveListener(StopShooting);
		}
	}

	private void Update()
	{
		HandleShooting();
	}

	private void HandleShooting()
	{
		if (!_enableShooting || _patterns == null || _patterns.Count == 0)
		{
			return;
		}

		if (_isWaitingForDelay)
		{
			_delayTimer -= Time.deltaTime;
			if (_delayTimer > 0f)
			{
				return;
			}
			_isWaitingForDelay = false;
		}

		PatternSO currentPattern = _patterns[_patternIndex];
		AttackEntry entry = currentPattern.Attacks[_attackIndex];

		// Fire the attack
		if (entry.Attack != null)
		{
			Vector2 origin = _shootPoints[_shootPointIndex].position;
			BulletManager.Instance.FireAttack(entry.Attack, origin);
			_shootPointIndex = (_shootPointIndex + 1) % _shootPoints.Count;
		}

		// Advance to the next attack
		_attackIndex++;
		if (_attackIndex >= currentPattern.Attacks.Count)
		{
			_attackIndex = 0;
			_patternIndex = (_patternIndex + 1) % _patterns.Count;
		}

		// Apply delay after this attack
		if (entry.DelayAfter > 0f)
		{
			_delayTimer = entry.DelayAfter;
			_isWaitingForDelay = true;
		}
	}

	/* Public Methods */

	[Button]
	public void StartShooting()
	{
		_enableShooting = true;
		_patternIndex = 0;
		_attackIndex = 0;
		_isWaitingForDelay = false;
	}

	[Button]
	public void StopShooting()
	{
		_enableShooting = false;
		_isWaitingForDelay = false;
		_delayTimer = 0f;
	}
}
