using UnityEngine;

public class MeleeDamage : MonoBehaviour
{
	[Header("Damage")]
	public float Damage = 10f;
	public bool ProcIFrames = true;

	[Header("Knockback")]
	public bool HasKnockback = true;
	public float Force = 20f;
	public KnockbackDirection Direction = KnockbackDirection.FromHitPosition;
	public Vector2 CustomDirection = Vector2.right; //placeholder

	[Header("Targeting")]
	public CombatTeam Team = CombatTeam.Fish;
	public bool HitOnce = true;

	[Header("References")]
	public GameObject Source;

	private Damageable _targetHit;

	public void SetSource(GameObject src)
	{
		Source = src;
	}

	public void ResetHit()
	{
		_targetHit = null;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.TryGetComponent(out Damageable damageable))
		{
			return;
		}

		// Skip if same team
		if ((damageable.Team & Team) != 0)
		{
			return;
		}

		// Skip if already hit this target
		if (HitOnce && _targetHit == damageable)
		{
			return;
		}

		Vector2 knockbackVector = Vector2.zero;
		if (HasKnockback && Force > 0f)
		{
			knockbackVector = CalculateKnockback(other.gameObject);
		}

		DamageData damageData = DamageSource.CreateDamageDataStatic(
			Damage,
			Source != null ? Source : gameObject,
			other.gameObject,
			other.ClosestPoint(transform.position),
			ProcIFrames,
			HasKnockback,
			knockbackVector
		);

		if (damageable.Damage(damageData))
		{
			_targetHit = damageable;
		}
	}

	private Vector2 CalculateKnockback(GameObject target)
	{
		GameObject knockbackSource = Source != null ? Source : gameObject;
		Vector2 direction = Direction switch
		{
			KnockbackDirection.FromSource => (
				(Vector2)target.transform.position - (Vector2)knockbackSource.transform.position
			).normalized,
			KnockbackDirection.FromHitPosition => (
				(Vector2)target.transform.position - (Vector2)transform.position
			).normalized,
			KnockbackDirection.Custom => (Vector2)transform.TransformDirection(CustomDirection).normalized,
			_ => Vector2.zero,
		};
		return direction * Force;
	}
}
