using System.Collections.Generic;
using UnityEngine;

public class BombDamage : MonoBehaviour
{
	[HideInInspector]
	public float damage = 50f;

	private HashSet<HealthController> damagedPlayers = new HashSet<HealthController>();

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			HealthController playerHealth = other.GetComponent<HealthController>();
			if (playerHealth != null && !damagedPlayers.Contains(playerHealth))
			{
				playerHealth.TakeDamage(damage);
				damagedPlayers.Add(playerHealth);
			}
		}
	}

	public void OnTriggerStay2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			HealthController playerHealth = other.GetComponent<HealthController>();
			if (playerHealth != null && !damagedPlayers.Contains(playerHealth))
			{
				playerHealth.TakeDamage(damage);
				damagedPlayers.Add(playerHealth);
			}
		}
	}
}
