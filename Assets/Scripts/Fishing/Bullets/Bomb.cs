using System.Collections;
using UnityEngine;

public class Bomb : BulletPattern
{
	[SerializeField]
	private float warningRadius = 2f;

	[SerializeField]
	private float explosionRadius = 3f;

	[SerializeField]
	private float warningTime = 1.5f;

	[SerializeField]
	private float activeTime = 0.5f;

	[SerializeField]
	private float damage = 50f;

	[SerializeField]
	private Color warningColor = new Color(1f, 0.5f, 0f, 0.3f);

	[SerializeField]
	private Color explosionColor = new Color(1f, 0.3f, 0f, 0.8f);

	[SerializeField]
	private bool spawnAtPlayer = true;

	[SerializeField]
	private Vector2 staticOffset = Vector2.zero;

	public override void Fire(Vector2 origin, Vector2? playerPos = null)
	{
		Vector2 center = origin;

		if (spawnAtPlayer && playerPos != null)
		{
			center = playerPos.Value;
		}
		else
		{
			center = origin + staticOffset;
		}

		StartCoroutine(BombCoroutine(center));
	}

	private IEnumerator BombCoroutine(Vector2 center)
	{
		// Create warning circle
		GameObject warning = CreateCircleVisual(center, warningRadius, warningColor);
		warning.layer = LayerMask.NameToLayer("Default");

		yield return new WaitForSeconds(warningTime);

		// Replace with explosion
		Destroy(warning);
		GameObject explosion = CreateCircleVisual(center, explosionRadius, explosionColor);

		CircleCollider2D collider = explosion.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = explosionRadius;

		BombDamage damageScript = explosion.AddComponent<BombDamage>();
		damageScript.damage = damage;

		yield return new WaitForSeconds(activeTime);

		Destroy(explosion);
	}

	private GameObject CreateCircleVisual(Vector2 center, float radius, Color color)
	{
		GameObject circle = new GameObject("Bomb");
		circle.transform.position = center;

		SpriteRenderer sr = circle.AddComponent<SpriteRenderer>();
		sr.sprite = CreateCircleSprite(radius);
		sr.color = color;
		sr.sortingOrder = 10;

		return circle;
	}

	private Sprite CreateCircleSprite(float radius)
	{
		int resolution = Mathf.Max(32, Mathf.RoundToInt(radius * 100));
		Texture2D texture = new Texture2D(resolution, resolution);
		Color[] pixels = new Color[resolution * resolution];

		Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
		float radiusPixels = resolution / 2f;

		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				float distance = Vector2.Distance(new Vector2(x, y), center);
				pixels[y * resolution + x] = distance <= radiusPixels ? Color.white : Color.clear;
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100f);
	}
}
