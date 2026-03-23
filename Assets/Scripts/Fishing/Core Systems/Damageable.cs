using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed on anything that can take damage.
/// Fires events that other systems (HealthController, visual effects, etc.) can listen to.
/// </summary>
public class Damageable : MonoBehaviour
{
	// Global registry of all active Damageables
	private static HashSet<Damageable> _allDamageables = new();

	/// <summary>
	/// Returns all active Damageables in the scene.
	/// </summary>
	public static IEnumerable<Damageable> AllDamageables => _allDamageables;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatics()
	{
		_allDamageables = new();
	}

	[Header("References (optional)")]
	[Tooltip("Whether or not this damageable is tied to something that has health")]
	[SerializeField]
	private HealthController _health;

	[Header("Parameters")]
	[Tooltip("Whether or not this gameobject can currently be damaged")]
	[SerializeField]
	private bool _isDamageable = true;

	[Tooltip("Whether or not this gameobject automatically handles damage sources")]
	[SerializeField]
	private bool _handleHealthLogicAutomatically = true;

	[Tooltip("Damage multiplier applied to all incoming damage (for resistances/vulnerabilities)")]
	[SerializeField]
	private float _damageMultiplier = 1f;

	[SerializeField]
	private CombatTeam _team = CombatTeam.None;

	public bool IsDamageable => _isDamageable;
	public bool HasHealth => _health != null;
	public CombatTeam Team => _team;
	public HealthController Health => _health;

	public float DamageMultiplier
	{
		get => _damageMultiplier;
		set => _damageMultiplier = value;
	}

	/// <summary>
	/// Invoked before damage is applied. Return false from any listener to block damage.
	/// Useful for shields, counters, invincibility effects, etc.
	/// </summary>
	public event Func<DamageData, bool> OnValidateDamage;

	/// <summary>
	/// Invoked when this object takes damage, after validation but before health is reduced.
	/// Parameters: DamageData
	/// </summary>
	public event Action<DamageData> OnDamaged;

	/// <summary>
	/// Invoked after health is reduced (only fires if HasHealth is true).
	/// Parameters: DamageData
	/// </summary>
	public event Action<DamageData> OnHealthDamaged;

	/// <summary>
	/// Invoked when damage is blocked (e.g., when IsDamageable is false or validation fails).
	/// Parameters: DamageData
	/// </summary>
	public event Action<DamageData> OnDamageBlocked;

	/// <summary>
	/// Applies damage to this Damageable.
	/// </summary>
	/// <param name="damageData">Data about the damage being applied</param>
	/// <returns>True if damage was applied, false if blocked</returns>
	public bool Damage(DamageData damageData)
	{
		if (!_isDamageable)
		{
			OnDamageBlocked?.Invoke(damageData);
			return false;
		}

		if (_handleHealthLogicAutomatically && HasHealth && _health.IsInvincible)
		{
			OnDamageBlocked?.Invoke(damageData);
			return false;
		}

		if (OnValidateDamage != null)
		{
			foreach (Func<DamageData, bool> validator in OnValidateDamage.GetInvocationList())
			{
				if (!validator(damageData))
				{
					OnDamageBlocked?.Invoke(damageData);
					return false; // Damage blocked by ability/skill
				}
			}
		}

		DamageData modifiedData = damageData;
		modifiedData.Amount *= _damageMultiplier;

		OnDamaged?.Invoke(modifiedData);

		if (HasHealth && _handleHealthLogicAutomatically)
		{
			_health.TakeDamage(modifiedData.Amount, damageData.ProcIFrames);
			OnHealthDamaged?.Invoke(modifiedData);
		}

		return true;
	}

	/// <summary>
	/// Convenience overload that accepts a DamageSource component.
	/// </summary>
	public bool Damage(DamageSource damageSource, Vector2? hitPosition = null, float damageMultiplier = 1f)
	{
		if (damageSource == null)
		{
			Debug.LogWarning("Attempted to damage with null DamageSource");
			return false;
		}

		DamageData data = damageSource.CreateDamageData(gameObject, hitPosition, damageMultiplier);
		return Damage(data);
	}

	public void SetDamageable(bool damageable)
	{
		_isDamageable = damageable;
	}

	private void OnEnable()
	{
		_allDamageables.Add(this);
	}

	private void OnDisable()
	{
		_allDamageables.Remove(this);
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if (_isDamageable)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, Vector2.one * 0.3f);
		}
	}

#endif
}
