using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zoning attack that damages Damageables either inside or outside a defined zone.
/// Attach to a child GameObject with one or more Collider2D components (set as triggers).
/// Supports composite colliders and dynamic collider changes.
/// </summary>
public class ZoningAttack : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Damage source that defines damage properties")]
	[SerializeField]
	private DamageSource _damageSource;

	[Header("Zone Settings")]
	[Tooltip("If false, damages outside the zone. If true, damages inside the zone.")]
	[SerializeField]
	private bool _invertZone = false;

	[Tooltip("Time in seconds between damage ticks")]
	[SerializeField]
	private float _damageInterval = 0.1f;

	[Tooltip("Grace period in seconds before first damage is applied after entering/leaving the danger state")]
	[SerializeField]
	private float _gracePeriod = 0.1f;

	[Tooltip("Whether the zone is currently tracking and damaging targets")]
	[SerializeField]
	private bool _trackingEnabled = true;

	private readonly HashSet<Damageable> _damageablesInZone = new(); // INSIDE the collider, regardless of invert
	private readonly Dictionary<Damageable, float> _lastDamageTimes = new();
	private readonly Dictionary<Damageable, float> _damageEnterTimes = new(); // first enter time; for grace period

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Damageable damageable))
		{
			_damageablesInZone.Add(damageable);
			// always reset, regardless of enter/exit. damageEnterTime updated in TryDamage()
			_damageEnterTimes.Remove(damageable);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.TryGetComponent(out Damageable damageable))
		{
			_damageablesInZone.Remove(damageable);
			// always reset, regardless of enter/exit. damageEnterTime updated in TryDamage()
			_damageEnterTimes.Remove(damageable);
		}
	}

	private void FixedUpdate()
	{
		if (!_trackingEnabled || _damageSource == null)
		{
			return;
		}

		if (_invertZone)
		{
			foreach (Damageable damageable in _damageablesInZone)
			{
				if (damageable == null || !damageable.IsDamageable)
				{
					continue;
				}
				TryDamage(damageable);
			}
		}
		else
		{
			// Damage all Damageables in the scene that are NOT in the zone
			foreach (Damageable damageable in Damageable.AllDamageables)
			{
				if (damageable == null || !damageable.IsDamageable || _damageablesInZone.Contains(damageable))
				{
					continue;
				}

				TryDamage(damageable);
			}
		}
	}

	/// <summary>
	/// Attempts to damage a Damageable, respecting the grace period and damage interval.
	/// </summary>
	private bool TryDamage(Damageable damageable)
	{
		if (damageable == null || _damageSource == null)
		{
			return false;
		}

		// update damageEnterTimes lazily (here)
		if (!_damageEnterTimes.ContainsKey(damageable))
		{
			_damageEnterTimes[damageable] = Time.time;
		}

		float timeInDangerState = Time.time - _damageEnterTimes[damageable];
		if (timeInDangerState < _gracePeriod)
		{
			return false;
		}

		if (_lastDamageTimes.TryGetValue(damageable, out float lastDamageTime))
		{
			float timeSinceLastDamage = Time.time - lastDamageTime;
			if (timeSinceLastDamage < _damageInterval)
			{
				return false;
			}
		}

		// Apply damage at the damageable's position
		Vector2 hitPosition = damageable.transform.position;
		bool success = _damageSource.ApplyDamage(damageable, hitPosition);

		if (success)
		{
			_lastDamageTimes[damageable] = Time.time;
		}

		return success;
	}

	/// <summary>
	/// Enables or disables the zoning attack.
	/// </summary>
	public void SetEnabled(bool enabled)
	{
		_trackingEnabled = enabled;
	}

	/// <summary>
	/// Clears all tracked Damageables and damage times.
	/// Useful for resetting the attack state.
	/// </summary>
	public void Reset()
	{
		_damageablesInZone.Clear();
		_lastDamageTimes.Clear();
		_damageEnterTimes.Clear();
	}

	private void OnDisable()
	{
		Reset();
	}
}
