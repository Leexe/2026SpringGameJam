using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Represents a source of damage (projectiles, hazards, attacks, etc.).
/// Can be attached to GameObjects or used to create damage data programmatically.
/// External scripts (GenericAttack, ZoningAttack, etc.) handle timing/area logic.
/// </summary>
public class DamageSource : MonoBehaviour
{
	[Header("Damage Properties")]
	[Tooltip("Base damage amount dealt")]
	[SerializeField]
	private float _baseDamage = 10f;

	[Tooltip("Does this attack activate invincibility frames when it hits the target")]
	[SerializeField]
	private bool _procIFrames = true;

	[Tooltip("GameObject that caused this damage (e.g., the player, a fish)")]
	[SerializeField]
	private GameObject _source;

	[Tooltip("Damage will not apply to damageables on the same team")]
	[SerializeField]
	private CombatTeam _team = CombatTeam.None;

	[Header("Knockback (optional)")]
	[Tooltip("Enable knockback on damage")]
	[SerializeField]
	private bool _hasKnockback = false;

	[Tooltip("Knockback force magnitude")]
	[SerializeField]
	[ShowIf("_hasKnockback")]
	private float _knockbackForce = 5f;

	[Tooltip("Direction of knockback (None = from source to target)")]
	[SerializeField]
	[ShowIf("_hasKnockback")]
	private KnockbackDirection _knockbackDirection = KnockbackDirection.FromSource;

	[Tooltip("Custom knockback direction (used if KnockbackDirection is Custom)")]
	[SerializeField]
	[ShowIf("_hasKnockback")]
	private Vector2 _customKnockbackDirection = Vector2.right;

	public float BaseDamage
	{
		get => _baseDamage;
		set => _baseDamage = value;
	}

	public GameObject Source
	{
		get => _source;
		set => _source = value;
	}

	public bool HasKnockback
	{
		get => _hasKnockback;
		set => _hasKnockback = value;
	}

	public float KnockbackForce
	{
		get => _knockbackForce;
		set => _knockbackForce = value;
	}

	public CombatTeam Team
	{
		get => _team;
		set => _team = value;
	}

	private void Awake()
	{
		// Default source to this GameObject if not set
		if (_source == null)
		{
			_source = gameObject;
		}
	}

	/// <summary>
	/// Creates damage data for applying to a Damageable.
	/// </summary>
	public DamageData CreateDamageData(GameObject target, Vector2? hitPosition = null, float damageMultiplier = 1f)
	{
		Vector2 finalHitPosition = hitPosition ?? (Vector2)target.transform.position;
		Vector2 knockbackVector = Vector2.zero;

		if (_hasKnockback)
		{
			knockbackVector = CalculateKnockback(target, finalHitPosition);
		}

		return new DamageData
		{
			Amount = _baseDamage * damageMultiplier,
			HitPosition = finalHitPosition,
			Source = _source,
			Target = target,
			ProcIFrames = _procIFrames,
			HasKnockback = _hasKnockback,
			KnockbackVector = knockbackVector,
		};
	}

	public bool ApplyDamage(Damageable damageable, Vector2? hitPosition = null, float damageMultiplier = 1f)
	{
		if (damageable == null)
		{
			return false;
		}
		if ((damageable.Team & _team) != 0)
		{
			return false;
		}

		DamageData data = CreateDamageData(damageable.gameObject, hitPosition, damageMultiplier);
		return damageable.Damage(data);
	}

	private Vector2 CalculateKnockback(GameObject target, Vector2 hitPosition)
	{
		Vector2 direction;

		switch (_knockbackDirection)
		{
			case KnockbackDirection.FromSource:
				Vector2 sourcePos = _source != null ? (Vector2)_source.transform.position : hitPosition;
				direction = ((Vector2)target.transform.position - sourcePos).normalized;
				break;

			case KnockbackDirection.FromHitPosition:
				direction = ((Vector2)target.transform.position - hitPosition).normalized;
				break;

			case KnockbackDirection.Custom:
				direction = _customKnockbackDirection.normalized;
				break;

			default:
				direction = Vector2.zero;
				break;
		}

		return direction * _knockbackForce;
	}

	/// <summary>
	/// create damage data without a referenced damage instance.
	/// for procedural dmg / quick dmg applications
	/// </summary>
	public static DamageData CreateDamageDataStatic(
		float damage,
		GameObject source,
		GameObject target,
		Vector2 hitPosition,
		bool procIFrames,
		bool hasKnockback = false,
		Vector2 knockbackVector = default
	)
	{
		return new DamageData
		{
			Amount = damage,
			HitPosition = hitPosition,
			Source = source,
			Target = target,
			ProcIFrames = procIFrames,
			HasKnockback = hasKnockback,
			KnockbackVector = knockbackVector,
		};
	}
}

/// <summary>
/// contains all information about a damage instance
/// </summary>
[Serializable]
public struct DamageData
{
	public float Amount;
	public Vector2 HitPosition;
	public GameObject Source;
	public GameObject Target;
	public bool ProcIFrames;
	public bool HasKnockback;
	public Vector2 KnockbackVector;
}

public enum KnockbackDirection
{
	FromSource, // damage source to target
	FromHitPosition, // hit position to target
	Custom,
}
