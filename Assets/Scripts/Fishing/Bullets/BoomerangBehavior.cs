using System;
using UnityEngine;

public class BoomerangBehavior : MonoBehaviour
{
	private Vector2 _velocity;
	private float _curvature;
	private float _spinSpeed;
	private float _damage;
	private float _spawnTime;
	private float _lifetime;
	public event Action OnDestroyed;

	public void Initialize(Vector2 initialDirection, float speed, float curve, float spin, float life, float dmg)
	{
		_velocity = initialDirection * speed;
		_curvature = curve;
		_spinSpeed = spin;
		_lifetime = life;
		_damage = dmg;
		_spawnTime = Time.time;
	}

	private void Update()
	{
		transform.position += (Vector3)_velocity * Time.deltaTime;

		// Apply curve
		Vector2 perpendicular = new Vector2(-_velocity.y, _velocity.x).normalized;
		_velocity += perpendicular * _curvature * Time.deltaTime;

		transform.Rotate(0, 0, _spinSpeed * Time.deltaTime);
		if (Time.time - _spawnTime > _lifetime)
		{
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			HealthController playerHealth = other.GetComponent<HealthController>();
			if (playerHealth != null)
			{
				playerHealth.TakeDamage(_damage);
				// for checking collision
				Destroy(gameObject);
			}
		}
	}

	private void OnBecameInvisible()
	{
		Destroy(gameObject);
	}

	private void OnDestroy()
	{
		OnDestroyed?.Invoke();
	}
}
