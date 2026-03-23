using UnityEngine;

public class Boomerang : BulletPattern
{
	[SerializeField]
	private int _bulletCount = 1;

	[SerializeField]
	private float _curve = 2f;

	[SerializeField]
	private float _spinSpeed = 360f;

	[SerializeField]
	private float _lifetime = 3f;

	[SerializeField]
	private Sprite _customSprite = null;

	[SerializeField]
	private bool _aimAtPlayer = false;

	[SerializeField]
	private float _spreadAngle = 30f; // If multiple boomerangs

	[SerializeField]
	private Vector2 _staticDirection = Vector2.right;

	public override void Fire(Vector2 origin, Vector2? playerPos = null)
	{
		Vector2 baseDirection = _staticDirection.normalized;

		if (_aimAtPlayer && playerPos != null)
		{
			baseDirection = (playerPos.Value - origin).normalized;
		}

		// Spawn multiple boomerangs with spread
		for (int i = 0; i < _bulletCount; i++)
		{
			float angle = 0f;
			if (_bulletCount > 1)
			{
				float angleStep = _spreadAngle / (_bulletCount - 1);
				angle = -_spreadAngle / 2f + (i * angleStep);
			}

			Vector2 direction = RotateVector(baseDirection, angle);
			SpawnBoomerang(origin, direction);
		}
	}

	private void SpawnBoomerang(Vector2 origin, Vector2 direction)
	{
		var boomerang = new GameObject("Boomerang");
		boomerang.transform.position = origin;

		SpriteRenderer sr = boomerang.AddComponent<SpriteRenderer>();
		sr.sprite = _customSprite != null ? _customSprite : CreateCrescentSprite();
		sr.sortingOrder = 10;

		// Template collider
		CircleCollider2D collider = boomerang.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = 0.3f; // Arbitrary radius
		BoomerangBehavior behavior = boomerang.AddComponent<BoomerangBehavior>();
		behavior.Initialize(direction, BulletSpeed, _curve, _spinSpeed, _lifetime, BulletDamage);
	}

	/* Creates a crescent-shaped sprite if no custom sprite is provided, placeholder for now */
	private Sprite CreateCrescentSprite()
	{
		int resolution = 64;
		var texture = new Texture2D(resolution, resolution);
		Color[] pixels = new Color[resolution * resolution];

		Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
		float outerRadius = resolution / 2f;
		float innerRadius = outerRadius * 0.85f;
		Vector2 innerCenter = center + new Vector2(resolution * 0.15f, 0);

		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				var pos = new Vector2(x, y);
				float distOuter = Vector2.Distance(pos, center);
				float distInner = Vector2.Distance(pos, innerCenter);

				bool inCrescent = distOuter <= outerRadius && distInner > innerRadius;
				pixels[y * resolution + x] = inCrescent ? Color.white : Color.clear;
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100f);
	}
}
