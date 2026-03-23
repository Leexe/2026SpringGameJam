using UnityEngine;

public class BulletIntegrationHacks : MonoBehaviour
{
	[SerializeField]
	private Bullet _bullet;

	[SerializeField]
	private GenericAttack _genericAttack;

	private void Update()
	{
		// this is bad
		_genericAttack.DamageSource.BaseDamage = _bullet.Damage;
	}

	private void OnEnable()
	{
		_genericAttack.OnDamageApplied += OnHitSomething;
		// _genericAttack.OnDamageBlocked; not handled (oops)
	}

	private void OnDisable()
	{
		if (_genericAttack != null)
		{
			_genericAttack.OnDamageApplied -= OnHitSomething;
		}
	}

	private void OnHitSomething(Damageable damageable, DamageData data)
	{
		// break on hit
		_bullet.Velocity = Vector2.zero;
		_bullet.Burst();
	}
}
