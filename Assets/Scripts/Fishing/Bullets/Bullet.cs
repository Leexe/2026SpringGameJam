using UnityEngine;

public class Bullet : MonoBehaviour
{
	[HideInInspector]
	public Vector2 Velocity;

	[HideInInspector]
	public float Damage = 10f;

	[HideInInspector]
	public float Lifetime = 5f;

	private float _spawnTime;
	private BulletPool _pool;
	private Animator _animator;
	private bool _isBursting;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void Initialize(BulletPool bulletPool, Vector2 vel, float dmg, float life)
	{
		_pool = bulletPool;
		Velocity = vel;
		Damage = dmg;
		Lifetime = life;
		_spawnTime = Time.time;
		_isBursting = false;

		if (_animator != null)
		{
			_animator.SetTrigger("Spawn");
		}
	}

	private void Update()
	{
		//stops moving during burst animation
		if (_isBursting)
		{
			return;
		}
		transform.position += (Vector3)Velocity * Time.deltaTime;

		if (!_isBursting && Time.time - _spawnTime > Lifetime)
		{
			Burst();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && !_isBursting)
		{
			HealthController playerHealth = other.GetComponent<HealthController>();
			if (playerHealth != null)
			{
				playerHealth.TakeDamage(Damage);
			}

			//bullet stops moving after hitting player
			Velocity = Vector2.zero;
			Burst();
		}
	}

	public void Burst()
	{
		if (_isBursting)
		{
			return;
		}
		_isBursting = true;

		if (_animator != null)
		{
			_animator.SetTrigger("Burst");
		}
		else
		{
			ReturnToPool();
		}
	}

	//called by animation event
	public void OnBurstComplete()
	{
		ReturnToPool();
	}

	private void ReturnToPool()
	{
		_isBursting = false;

		if (_animator != null)
		{
			_animator.ResetTrigger("Burst");
			_animator.Play("Idle", 0, 0f);
			_animator.Update(0f);
		}

		if (_pool != null)
		{
			_pool.ReturnBullet(this);
		}
	}
}
