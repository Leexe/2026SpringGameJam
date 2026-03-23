using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
	[Header("Max Health")]
	[Tooltip("How much health the target has")]
	[SerializeField]
	private float _defaultMaxHealth = 100f;

	[Header("Regeneration")]
	[Tooltip("How much health the target heals per second")]
	[SerializeField]
	private float _defaultRegen = 1f;

	[Tooltip("How long after being hit does regeneration start")]
	[SerializeField]
	private float _defaultRegenDelay;

	[Header("Invincibility")]
	[Tooltip("How long the i-frames last after getting damaged")]
	[SerializeField]
	private float _defaultInvincibilityTime;

	private float _regeneration;
	private float _maxHealth;
	private float _health;
	private float _hurtInvincibilityTime;
	private float _invincibilityTime;
	private bool _isDead;
	private float _regenDelay;
	private float _regenDelayTimer;
	private bool CanRegenerate => _regenDelayTimer <= 0;
	private bool CanTakeDamage => !IsInvincible;
	public bool IsInvincible => _invincibilityTime > 0;

	// Events
	[HideInInspector]
	public UnityEvent<float, float> OnRegen; // Parameters: health, maxHealth

	[HideInInspector]
	public UnityEvent<float, float> OnHeal; // Parameters: health, maxHealth

	[HideInInspector]
	public UnityEvent<float, float> OnDamage; // Parameters: health, maxHealth

	[HideInInspector]
	public UnityEvent<float, float> OnRevive; // Parameters: health, maxHealth

	[HideInInspector]
	public UnityEvent OnDeath;

	// Getters
	public bool IsAlive => !_isDead;
	public bool IsDead => _isDead;
	public bool IsFullHealth => _maxHealth <= _health;
	public float GetHealth => _health;
	public float GetMaxHealth => _maxHealth;
	public float GetNormalizedHealth => _health / _maxHealth;

	private void Awake()
	{
		_maxHealth = _defaultMaxHealth;
		_health = _maxHealth;
		_regeneration = _defaultRegen;
		_regenDelay = _defaultRegenDelay;
		_hurtInvincibilityTime = _defaultInvincibilityTime;
	}

	private void Update()
	{
		if (!_isDead)
		{
			HandleRegeneration(Time.deltaTime);
		}
		_regenDelayTimer -= Time.deltaTime;
		_invincibilityTime -= Time.deltaTime;
	}

	private void HandleRegeneration(float deltaTime)
	{
		if (CanRegenerate && !IsFullHealth && !_isDead)
		{
			HealHealth(_regeneration * deltaTime);
			OnRegen?.Invoke(_health, _maxHealth);
		}
	}

	private void HealHealth(float healing)
	{
		if (_isDead)
		{
			return;
		}
		_health += healing;
		_health = Mathf.Min(_health, _maxHealth);
	}

	/// <summary>
	/// Make the target take the given amount of damage and mark the target is dead when health < 0
	/// </summary>
	/// <param name="damage">Damage to take</param>
	/// <param name="triggerInvincibility">Should this damage proc invincibility</param>
	public void TakeDamage(float damage, bool triggerInvincibility = true)
	{
		// Don't take damage if the target has died
		if (!_isDead && CanTakeDamage)
		{
			_health -= damage;
			_regenDelayTimer = _regenDelay;

			if (triggerInvincibility)
			{
				_invincibilityTime = _hurtInvincibilityTime;
			}

			// If the target is below a certain threshold of health, trigger death
			if (_health <= 0.01f)
			{
				_health = 0f;
				_isDead = true;

				OnDamage?.Invoke(_health, _maxHealth);
				OnDeath?.Invoke();
			}
			// Else, damage them
			else
			{
				OnDamage?.Invoke(_health, _maxHealth);
			}
		}
	}

	/// <summary>
	/// Revives the target and sets their health to the health amount
	/// </summary>
	/// <param name="healthNormalized">A value from 0-1 that lerps between 0 and max health, healing the target for that amount</param>
	public void Revive(float healthNormalized = 1f)
	{
		_isDead = false;
		SetHealth(healthNormalized);
		OnRevive?.Invoke(_health, _maxHealth);
	}

	/// <summary>
	/// Changes the regeneration rate
	/// </summary>
	/// <param name="regenRate">Amount of healing per second</param>
	public void SetRegeneration(float regenRate)
	{
		_regeneration = regenRate;
	}

	/// <summary>
	/// Sets the health to a value between 0 and _maxHealth
	/// </summary>
	/// <param name="healthNormalized">A value from 0-1 that lerps between 0 and max health, setting the target's health to that amount</param>
	public void SetHealth(float healthNormalized = 1f)
	{
		_health = Mathf.Clamp(_maxHealth * healthNormalized, 0, _maxHealth);
		if (_health <= 0)
		{
			_isDead = true;
			OnDeath?.Invoke();
		}
		else
		{
			_isDead = false;
		}
	}

	/// <summary>
	/// Sets the max health
	/// </summary>
	/// <param name="maxHealth">How much max health the target should have</param>
	/// <param name="healHealthGained">Heals the target if the new maxHealth is greater than their current maxHealth</param>
	public void SetMaxHealth(float maxHealth, bool healHealthGained = true)
	{
		float oldMaxHealth = _maxHealth;
		_maxHealth = maxHealth;
		if (healHealthGained)
		{
			if (_maxHealth > oldMaxHealth)
			{
				HealHealth(_maxHealth - oldMaxHealth);
			}
		}
	}

	/// <summary>
	/// Sets the amount of time it take before delay starts
	/// </summary>
	/// <param name="delay">Time in seconds for delay</param>
	public void SetRegenDelay(float delay)
	{
		_regenDelay = delay;
	}

	/// <summary>
	/// Heals the target for the given amount
	/// </summary>
	/// <param name="amount">Amount to heal</param>
	public void Heal(float amount)
	{
		HealHealth(amount);

		OnHeal?.Invoke(_health, _maxHealth);
	}

	/// <summary>
	/// The duration of the invincibility after getting hit
	/// </summary>
	/// <param name="duration">Time in seconds for invincibility</param>
	public void SetHurtIframeTime(float duration)
	{
		_hurtInvincibilityTime = duration;
	}

	/// <summary>
	/// Give the target duration of invincibility, invincibility less than the given amount is not applied
	/// </summary>
	/// <param name="duration">Time in seconds for invincibility</param>
	public void SetInvincibilityTime(float duration)
	{
		if (_invincibilityTime < duration)
		{
			_invincibilityTime = duration;
		}
	}
}
