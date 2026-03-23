using System;
using UnityEngine;

public class BiteDetectionZone : MonoBehaviour
{
	public event Action<Biteable> OnBiteableEntered;
	public event Action<Biteable> OnBiteableExited;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Biteable biteable))
		{
			OnBiteableEntered?.Invoke(biteable);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.TryGetComponent(out Biteable biteable))
		{
			OnBiteableExited?.Invoke(biteable);
		}
	}
}
