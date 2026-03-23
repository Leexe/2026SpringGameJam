using UnityEngine;

public class ClusterTrigger : MonoBehaviour
{
	public float Damage = 10f;
	public bool ProcIFrames = true;
	public CombatTeam Team = CombatTeam.Fish;

	public bool HasHit { get; private set; }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (HasHit)
		{
			return;
		}

		if (!other.TryGetComponent(out Damageable damageable))
		{
			return;
		}

		if ((damageable.Team & Team) != 0)
		{
			return;
		}

		DamageData damageData = DamageSource.CreateDamageDataStatic(
			Damage,
			gameObject,
			other.gameObject,
			other.ClosestPoint(transform.position),
			ProcIFrames,
			false,
			Vector2.zero
		);

		damageable.Damage(damageData);
		HasHit = true;
	}
}
