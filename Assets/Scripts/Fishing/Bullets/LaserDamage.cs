using System.Collections.Generic;
using UnityEngine;

public class LaserDamage : MonoBehaviour
{
	[HideInInspector]
	public float damagePerSecond = 20f;

	private HealthController _playerInside;
	private float _damageTimer = 0f;
	private const float DAMAGE_INTERVAL = 0.1f;

	private void Update()
	{
		if (_playerInside == null)
			return;

		_damageTimer += Time.deltaTime;
		if (_damageTimer >= DAMAGE_INTERVAL)
		{
			_playerInside.TakeDamage(damagePerSecond * DAMAGE_INTERVAL);
			_damageTimer = 0f;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			HealthController playerHealth = other.GetComponent<HealthController>();
			if (playerHealth != null)
			{
				_playerInside = playerHealth;
				_damageTimer = 0f;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			HealthController playerHealth = other.GetComponent<HealthController>();
			if (playerHealth == _playerInside)
			{
				_playerInside = null;
			}
		}
	}
}
