using UnityEngine;

public abstract class BulletPattern : MonoBehaviour
{
	[SerializeField]
	protected BulletPool BulletPool;

	[SerializeField]
	protected float BulletSpeed = 5f;

	[SerializeField]
	protected float BulletDamage = 10f;

	[SerializeField]
	protected float BulletLifetime = 5f;

	protected void SpawnBullet(Vector2 position, Vector2 velocity)
	{
		if (BulletPool != null)
		{
			BulletPool.GetBullet(position, velocity, BulletDamage, BulletLifetime);
		}
		else
		{
			Debug.LogError("BulletPool not assigned to pattern!");
		}
	}

	// Overload for custom damage and lifetime
	protected void SpawnBullet(Vector2 position, Vector2 velocity, float damage, float lifetime)
	{
		if (BulletPool != null)
		{
			BulletPool.GetBullet(position, velocity, damage, lifetime);
		}
		else
		{
			Debug.LogError("BulletPool not assigned to pattern!");
		}
	}

	protected Vector2 RotateVector(Vector2 v, float degrees)
	{
		float rad = degrees * Mathf.Deg2Rad;
		float cos = Mathf.Cos(rad);
		float sin = Mathf.Sin(rad);
		return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
	}

	//Override this in child classes
	public abstract void Fire(Vector2 origin, Vector2? playerPos = null);
}
