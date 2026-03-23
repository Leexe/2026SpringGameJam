using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic attack component that handles damage application for most use cases.
/// Works with bullets, lasers, melee attacks, hazards, etc.
/// Damageables entering the collision area take damage once, with optional cooldown for re-triggering.
/// </summary>
public class GenericAttack : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Collision area that detects Damageables")]
	[SerializeField]
	private Collider2D _collisionArea;

	[SerializeField]
	private DamageSource _damageSource;

	[Header("Retrigger Settings")]
	[Tooltip("Enable cooldown before the same target can be damaged again")]
	[SerializeField]
	private bool _enableRetriggerCooldown = false;

	[Tooltip("Time in seconds before a Damageable can be damaged again (0 = never retrigger)")]
	[SerializeField]
	private float _retriggerCooldown = 1f;

	public event Action<Damageable, DamageData> OnDamageApplied;
	public event Action<Damageable> OnDamageBlocked;

	public event Action OnDamageEnable;
	public event Action OnDamageDisable;

	private bool _enabled = true;

	public DamageSource DamageSource => _damageSource;

	// Tracks last hit time per Damageable
	private readonly Dictionary<Damageable, float> _lastHitTimes = new();

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Damageable damageable))
		{
			if (_enabled)
			{
				TryDamage(damageable, other.ClosestPoint(transform.position));
			}
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!_enabled)
		{
			return;
		}

		// Handle retrigger if enabled
		if (_enableRetriggerCooldown && _retriggerCooldown > 0f)
		{
			if (other.TryGetComponent(out Damageable damageable))
			{
				TryDamage(damageable, other.ClosestPoint(transform.position));
			}
		}
	}

	/// <summary>
	/// Attempts to damage a Damageable, respecting cooldown rules.
	/// </summary>
	private bool TryDamage(Damageable damageable, Vector2 hitPosition)
	{
		if (damageable == null || _damageSource == null)
		{
			return false;
		}

		// Check if on cooldown
		if (_lastHitTimes.TryGetValue(damageable, out float lastHitTime))
		{
			if (!_enableRetriggerCooldown)
			{
				// Never retrigger
				return false;
			}

			float timeSinceLastHit = Time.time - lastHitTime;
			if (timeSinceLastHit < _retriggerCooldown)
			{
				// Still on cooldown
				return false;
			}
		}

		// Apply damage
		bool success = _damageSource.ApplyDamage(damageable, hitPosition);

		if (success)
		{
			_lastHitTimes[damageable] = Time.time;

			// Get the damage data for the event
			DamageData data = _damageSource.CreateDamageData(damageable.gameObject, hitPosition);
			OnDamageApplied?.Invoke(damageable, data);
		}
		else
		{
			OnDamageBlocked?.Invoke(damageable);
		}

		return success;
	}

	/// <summary>
	/// Refreshes the attack, allowing all Damageables to be damaged again immediately.
	/// </summary>
	public void Refresh()
	{
		_lastHitTimes.Clear();
	}

	public void SetEnabled(bool enabled)
	{
		_enabled = enabled;
		if (_enabled)
		{
			OnDamageEnable?.Invoke();
		}
		else
		{
			OnDamageDisable?.Invoke();
		}
	}

	/// <summary>
	/// Refreshes cooldown for a specific Damageable.
	/// </summary>
	public void RefreshForTarget(Damageable damageable)
	{
		if (damageable != null && _lastHitTimes.ContainsKey(damageable))
		{
			_lastHitTimes.Remove(damageable);
		}
	}

	/// <summary>
	/// Checks if a Damageable is currently on cooldown.
	/// </summary>
	public bool IsOnCooldown(Damageable damageable)
	{
		if (damageable == null || !_lastHitTimes.ContainsKey(damageable))
		{
			return false;
		}

		if (!_enableRetriggerCooldown)
		{
			return true; // Permanently on cooldown
		}

		float timeSinceLastHit = Time.time - _lastHitTimes[damageable];
		return timeSinceLastHit < _retriggerCooldown;
	}

	private void OnDisable()
	{
		_lastHitTimes.Clear();
	}
}
