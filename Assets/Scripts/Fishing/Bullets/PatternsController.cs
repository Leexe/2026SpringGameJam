using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternsController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private BulletPool _bulletPool;

	[Header("Laser")]
	[SerializeField]
	private GameObject _laserPrefab;

	private List<GameObject> _activePatternObjects = new List<GameObject>();

	public void CleanupAll()
	{
		foreach (GameObject obj in _activePatternObjects)
		{
			if (obj != null)
			{
				Destroy(obj);
			}
		}
		_activePatternObjects.Clear();
		StopAllCoroutines();
	}

	private void RegisterPatternObject(GameObject obj)
	{
		if (obj != null)
		{
			_activePatternObjects.Add(obj);
		}
	}

	private void UnregisterPatternObject(GameObject obj)
	{
		_activePatternObjects.Remove(obj);
	}

	/// <summary>
	/// Shoots a spread of bullets from an origin point.
	/// </summary>
	/// <param name="count">Number of bullets to spawn.</param>
	/// <param name="damage">Damage per bullet.</param>
	/// <param name="spread">Total angle of the spread in degrees (e.g. 360 for radial).</param>
	/// <param name="bulletSpeed">Speed of the bullets.</param>
	/// <param name="lifetime">Lifetime of bullets in seconds.</param>
	/// <param name="angleOffset">Center angle of the spread.
	/// <param name="origin">Origin transform to spawn from.</param>
	/// <param name="target">Optional target. If set, angleOffset is relative to the direction to this target.</param>
	public void ShootBullet(
		int count,
		float damage,
		float spread,
		float bulletSpeed,
		float lifetime,
		float angleOffset,
		Transform origin,
		Transform target = null,
		bool randomAngleOffset = false
	)
	{
		Vector2 toPlayer = Vector2.zero;
		if (target != null)
		{
			toPlayer = (target.position - origin.position).normalized;
		}

		float angleStep = count > 1 ? spread / (count - 1) : 0;
		float startAngle;
		if (randomAngleOffset)
		{
			startAngle = Random.Range(-angleOffset / 2f, angleOffset / 2f) + (-spread / 2f);
		}
		else
		{
			startAngle = angleOffset + (-spread / 2f);
		}

		for (int i = 0; i < count; i++)
		{
			float angle = startAngle + (i * angleStep);
			Vector2 direction;
			if (toPlayer != Vector2.zero)
			{
				direction = RotateVector(toPlayer, angle);
			}
			else
			{
				direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
			}

			SpawnBullet(origin.position, direction * bulletSpeed, damage, lifetime);
		}
	}

	/// <summary>
	/// Spawns a wall of bullets.
	/// </summary>
	/// <param name="count">Number of bullets</param>
	/// <param name="damage">Damage per bullet</param>
	/// <param name="space">Spacing between bullets</param>
	/// <param name="wallAngle">Angle of the wall (relative to target direction if target exists, else absolute)</param>
	/// <param name="angleOffset">Shooting angle offset (relative to target direction if target exists, else absolute)</param>
	/// <param name="bulletSpeed">Speed of bullets</param>
	/// <param name="lifetime">Lifetime of bullets</param>
	/// <param name="origin">Origin transform</param>
	/// <param name="target">Optional target transform</param>
	public void ShootWall(
		int count,
		float damage,
		float space,
		float wallAngle,
		float angleOffset,
		float bulletSpeed,
		float lifetime,
		Transform origin,
		Transform target = null
	)
	{
		Vector2 shootDirection;
		Vector2 wallDirection;

		if (target != null)
		{
			Vector2 toTarget = (target.position - origin.position).normalized;
			shootDirection = RotateVector(toTarget, angleOffset);
			wallDirection = RotateVector(toTarget, wallAngle);
		}
		else
		{
			shootDirection = new Vector2(
				Mathf.Cos(angleOffset * Mathf.Deg2Rad),
				Mathf.Sin(angleOffset * Mathf.Deg2Rad)
			);
			wallDirection = new Vector2(Mathf.Cos(wallAngle * Mathf.Deg2Rad), Mathf.Sin(wallAngle * Mathf.Deg2Rad));
		}

		float totalWidth = (count - 1) * space;
		Vector2 startOffset = -wallDirection * (totalWidth / 2f);

		for (int i = 0; i < count; i++)
		{
			Vector2 spawnPos = (Vector2)origin.position + startOffset + (i * space * wallDirection);
			SpawnBullet(spawnPos, shootDirection * bulletSpeed, damage, lifetime);
		}
	}

	/* Helper Functions */

	private Vector2 RotateVector(Vector2 v, float degrees)
	{
		float rad = degrees * Mathf.Deg2Rad;
		float cos = Mathf.Cos(rad);
		float sin = Mathf.Sin(rad);
		return new Vector2((v.x * cos) - (v.y * sin), (v.x * sin) + (v.y * cos));
	}

	private void SpawnBullet(Vector2 position, Vector2 velocity, float bulletDamage = 10f, float bulletLifetime = 5f)
	{
		if (_bulletPool != null)
		{
			_bulletPool.GetBullet(position, velocity, bulletDamage, bulletLifetime);
		}
		else
		{
			Debug.LogError("BulletPool not assigned to pattern!");
		}
	}

	public void ShootLaser(
		float length,
		float width,
		float warningTime,
		float activeTime,
		float damagePerSecond,
		float turnRate,
		bool trackTarget,
		Transform origin,
		Transform target = null,
		Color? warningColor = null,
		Color? laserColor = null
	)
	{
		Color warnCol = warningColor ?? new Color(1f, 0f, 0f, 0.3f);
		Color activeCol = laserColor ?? new Color(1f, 0f, 0f, 1f);

		StartCoroutine(
			LaserCoroutine(
				length,
				width,
				warningTime,
				activeTime,
				damagePerSecond,
				turnRate,
				trackTarget,
				origin,
				target,
				warnCol,
				activeCol
			)
		);
	}

	private IEnumerator LaserCoroutine(
		float length,
		float width,
		float warningTime,
		float activeTime,
		float damagePerSecond,
		float turnRate,
		bool trackTarget,
		Transform origin,
		Transform target,
		Color? warningColor,
		Color? laserColor
	)
	{
		Vector2 initialOrigin = origin.position;
		Vector2 initialDirection;

		if (target != null)
		{
			initialDirection = ((Vector2)target.position - initialOrigin).normalized;
		}
		else
		{
			initialDirection = Vector2.right;
		}

		Vector2 currentDirection = initialDirection;

		if (_laserPrefab != null)
		{
			yield return AnimatedLaserCoroutine(
				length,
				width,
				warningTime,
				activeTime,
				damagePerSecond,
				turnRate,
				trackTarget,
				origin,
				target,
				initialDirection,
				warningColor,
				laserColor
			);
			yield break;
		}

		// Fallback
		Color warnCol = warningColor ?? new Color(1f, 0f, 0f, 0.3f);
		Color activeCol = laserColor ?? new Color(1f, 0f, 0f, 1f);

		// Create warning sprite
		GameObject warning = CreateLaserVisual(initialOrigin, currentDirection, length, width, warnCol);
		warning.layer = LayerMask.NameToLayer("Default");
		RegisterPatternObject(warning);

		// Track during warning phase
		float elapsed = 0f;
		while (elapsed < warningTime)
		{
			if (warning == null)
				yield break;
			elapsed += Time.deltaTime;

			Vector2 currentOrigin = origin != null ? (Vector2)origin.position : initialOrigin;
			warning.transform.position = currentOrigin;

			if (trackTarget && target != null)
			{
				Vector2 targetDirection = ((Vector2)target.position - currentOrigin).normalized;
				currentDirection = RotateTowards(currentDirection, targetDirection, turnRate * Time.deltaTime);

				float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
				warning.transform.rotation = Quaternion.Euler(0, 0, angle);
			}

			yield return null;
		}

		UnregisterPatternObject(warning);
		Destroy(warning);

		Vector2 laserOrigin = origin != null ? (Vector2)origin.position : initialOrigin;
		GameObject laser = CreateLaserVisual(laserOrigin, currentDirection, length, width, activeCol);
		RegisterPatternObject(laser);

		BoxCollider2D collider = laser.AddComponent<BoxCollider2D>();
		collider.isTrigger = true;
		collider.size = new Vector2(length, width);
		collider.offset = new Vector2(length / 2f, 0);

		LaserDamage damageScript = laser.AddComponent<LaserDamage>();
		damageScript.damagePerSecond = damagePerSecond;

		elapsed = 0f;
		while (elapsed < activeTime)
		{
			if (laser == null)
				yield break;
			elapsed += Time.deltaTime;

			Vector2 currentOrigin = origin != null ? (Vector2)origin.position : laserOrigin;
			laser.transform.position = currentOrigin;

			if (trackTarget && target != null)
			{
				Vector2 targetDirection = ((Vector2)target.position - currentOrigin).normalized;
				currentDirection = RotateTowards(currentDirection, targetDirection, turnRate * Time.deltaTime);

				float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
				laser.transform.rotation = Quaternion.Euler(0, 0, angle);
			}

			yield return null;
		}

		UnregisterPatternObject(laser);
		Destroy(laser);
	}

	private IEnumerator AnimatedLaserCoroutine(
		float length,
		float width,
		float warningTime,
		float activeTime,
		float damagePerSecond,
		float turnRate,
		bool trackTarget,
		Transform origin,
		Transform target,
		Vector2 initialDirection,
		Color? warningColor,
		Color? laserColor
	)
	{
		Color warnCol = warningColor.GetValueOrDefault(new Color(1f, 0f, 0f, 0.3f));
		Vector2 currentDirection = initialDirection;

		// Warning Phase
		GameObject warning = CreateLaserVisual(origin.position, currentDirection, length, width, warnCol);
		warning.layer = LayerMask.NameToLayer("Default");
		RegisterPatternObject(warning);

		float elapsed = 0f;
		while (elapsed < warningTime)
		{
			if (warning == null)
				yield break;

			elapsed += Time.deltaTime;
			if (origin != null)
			{
				warning.transform.position = origin.position;
			}

			// Track target
			if (trackTarget && target != null && origin != null)
			{
				Vector2 targetDirection = ((Vector2)target.position - (Vector2)origin.position).normalized;
				currentDirection = RotateTowards(currentDirection, targetDirection, turnRate * Time.deltaTime);

				float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
				warning.transform.rotation = Quaternion.Euler(0, 0, angle);
			}

			yield return null;
		}
		UnregisterPatternObject(warning);
		Destroy(warning);

		// Active Phase
		GameObject laserObj = Instantiate(_laserPrefab, origin.position, Quaternion.identity);
		RegisterPatternObject(laserObj);

		LaserVisual laserVisual = laserObj.GetComponent<LaserVisual>();
		if (laserVisual == null)
		{
			Debug.LogError("Laser prefab missing LaserVisual component!");
			UnregisterPatternObject(laserObj);
			Destroy(laserObj);
			yield break;
		}

		laserVisual.Initialize(length, width, damagePerSecond);
		laserVisual.SetDirection(currentDirection);
		laserVisual.SetWarningState(false);
		laserVisual.PlayForm(0.3f);

		elapsed = 0f;
		while (elapsed < activeTime)
		{
			if (laserObj == null)
				yield break;

			elapsed += Time.deltaTime;

			if (origin != null)
			{
				laserObj.transform.position = origin.position;
			}

			if (trackTarget && target != null && origin != null)
			{
				Vector2 targetDirection = ((Vector2)target.position - (Vector2)origin.position).normalized;
				currentDirection = RotateTowards(currentDirection, targetDirection, turnRate * Time.deltaTime);
				laserVisual.SetDirection(currentDirection);
			}

			yield return null;
		}

		// Break phase
		laserVisual.PlayBreak(0.3f);

		float breakElapsed = 0f;
		while (breakElapsed < 0.3f)
		{
			if (laserObj == null)
				yield break;

			breakElapsed += Time.deltaTime;
			if (origin != null)
			{
				laserObj.transform.position = origin.position;
			}

			yield return null;
		}

		if (laserObj != null)
		{
			UnregisterPatternObject(laserObj);
			Destroy(laserObj);
		}
	}

	private GameObject CreateLaserVisual(Vector2 origin, Vector2 direction, float length, float width, Color color)
	{
		GameObject laser = new GameObject("Laser");
		laser.transform.position = origin;

		// Calculate rotation to face direction
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		laser.transform.rotation = Quaternion.Euler(0, 0, angle);

		// Create sprite renderer
		SpriteRenderer sr = laser.AddComponent<SpriteRenderer>();
		sr.sprite = CreateRectangleSprite(length, width);
		sr.color = color;
		sr.sortingOrder = 10;

		return laser;
	}

	private Sprite CreateRectangleSprite(float spriteLength, float spriteWidth)
	{
		// Create simple white rectangle texture
		int pixelWidth = Mathf.Max(1, Mathf.RoundToInt(spriteLength * 100));
		int pixelHeight = Mathf.Max(1, Mathf.RoundToInt(spriteWidth * 100));

		Texture2D texture = new Texture2D(pixelWidth, pixelHeight);
		Color[] pixels = new Color[pixelWidth * pixelHeight];
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = Color.white;
		}
		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, pixelWidth, pixelHeight), new Vector2(0, 0.5f), 100f);
	}

	public void ShootBomb(
		float warningRadius,
		float explosionRadius,
		float warningTime,
		float activeTime,
		float damage,
		bool spawnAtTarget,
		Vector2 offset,
		Transform origin,
		Transform target = null,
		Color? warningColor = null,
		Color? explosionColor = null
	)
	{
		Color warnCol = warningColor ?? new Color(1f, 0.5f, 0f, 0.3f);
		Color explodeCol = explosionColor ?? new Color(1f, 0.3f, 0f, 0.8f);

		// Determine spawn center
		Vector2 center;
		if (spawnAtTarget && target != null)
		{
			center = target.position;
		}
		else
		{
			center = (Vector2)origin.position + offset;
		}

		StartCoroutine(
			BombCoroutine(center, warningRadius, explosionRadius, warningTime, activeTime, damage, warnCol, explodeCol)
		);
	}

	private IEnumerator BombCoroutine(
		Vector2 center,
		float warningRadius,
		float explosionRadius,
		float warningTime,
		float activeTime,
		float damage,
		Color warningColor,
		Color explosionColor
	)
	{
		// Create warning circle
		GameObject warning = CreateCircleVisual(center, warningRadius, warningColor);
		warning.layer = LayerMask.NameToLayer("Default");
		RegisterPatternObject(warning);

		yield return new WaitForSeconds(warningTime);

		// Replace with explosion
		UnregisterPatternObject(warning);
		Destroy(warning);

		GameObject explosion = CreateCircleVisual(center, explosionRadius, explosionColor);
		RegisterPatternObject(explosion);

		CircleCollider2D collider = explosion.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = explosionRadius;

		BombDamage damageScript = explosion.AddComponent<BombDamage>();
		damageScript.damage = damage;

		yield return new WaitForSeconds(activeTime);

		// Cleanup
		UnregisterPatternObject(explosion);
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

	public void ShootBoomerang(
		int count,
		float damage,
		float speed,
		float curve,
		float spinSpeed,
		float lifetime,
		float spreadAngle,
		float angleOffset,
		float colliderRadius,
		Transform origin,
		Transform target = null,
		Sprite customSprite = null
	)
	{
		Vector2 baseDirection;
		if (target != null)
		{
			baseDirection = ((Vector2)target.position - (Vector2)origin.position).normalized;
		}
		else
		{
			baseDirection = Vector2.right;
		}

		// Apply angle offset to base direction
		baseDirection = RotateVector(baseDirection, angleOffset);

		// Spawn multiple boomerangs with spread
		for (int i = 0; i < count; i++)
		{
			float angle = 0f;
			if (count > 1)
			{
				float angleStep = spreadAngle / (count - 1);
				angle = -spreadAngle / 2f + (i * angleStep);
			}

			Vector2 direction = RotateVector(baseDirection, angle);
			SpawnBoomerang(
				origin.position,
				direction,
				speed,
				curve,
				spinSpeed,
				lifetime,
				damage,
				colliderRadius,
				customSprite
			);
		}
	}

	private void SpawnBoomerang(
		Vector2 origin,
		Vector2 direction,
		float speed,
		float curve,
		float spinSpeed,
		float lifetime,
		float damage,
		float colliderRadius,
		Sprite customSprite
	)
	{
		GameObject boomerang = new GameObject("Boomerang");
		boomerang.transform.position = origin;
		RegisterPatternObject(boomerang);

		SpriteRenderer sr = boomerang.AddComponent<SpriteRenderer>();
		sr.sprite = customSprite != null ? customSprite : CreateCrescentSprite();
		sr.sortingOrder = 10;

		CircleCollider2D collider = boomerang.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = colliderRadius;

		BoomerangBehavior behavior = boomerang.AddComponent<BoomerangBehavior>();
		behavior.Initialize(direction, speed, curve, spinSpeed, lifetime, damage);
		behavior.OnDestroyed += () => UnregisterPatternObject(boomerang);
	}

	private Sprite CreateCrescentSprite()
	{
		int resolution = 64;
		Texture2D texture = new Texture2D(resolution, resolution);
		Color[] pixels = new Color[resolution * resolution];

		Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
		float outerRadius = resolution / 2f;
		float innerRadius = outerRadius * 0.85f;
		Vector2 innerCenter = center + new Vector2(resolution * 0.15f, 0);

		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				Vector2 pos = new Vector2(x, y);
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

	private Vector2 RotateTowards(Vector2 current, Vector2 target, float maxDegrees)
	{
		float currentAngle = Mathf.Atan2(current.y, current.x) * Mathf.Rad2Deg;
		float targetAngle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
		float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
		float rotationAmount = Mathf.Clamp(angleDiff, -maxDegrees, maxDegrees);

		float newAngle = (currentAngle + rotationAmount) * Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
	}

	public void ShootMelee(
		int shape,
		float width,
		float height,
		float arcAngle,
		float warningTime,
		float activeTime,
		float spawnDistance,
		Vector2 offset,
		float damage,
		float transparency,
		Transform origin,
		Transform target = null,
		float trackingLag = 0f,
		float knockbackForce = 0f,
		bool knockbackFromCenter = true,
		bool hitOnce = true
	)
	{
		StartCoroutine(
			MeleeCoroutine(
				shape,
				width,
				height,
				arcAngle,
				warningTime,
				activeTime,
				spawnDistance,
				offset,
				damage,
				transparency,
				origin,
				target,
				trackingLag,
				knockbackForce,
				knockbackFromCenter,
				hitOnce
			)
		);
	}

	private IEnumerator MeleeCoroutine(
		int shape,
		float width,
		float height,
		float arcAngle,
		float warningTime,
		float activeTime,
		float spawnDistance,
		Vector2 offset,
		float damage,
		float transparency,
		Transform origin,
		Transform target,
		float trackingLag,
		float knockbackForce,
		bool knockbackFromCenter,
		bool hitOnce
	)
	{
		Vector2 direction = Vector2.right;
		Vector2 currentDirection = Vector2.right;
		if (target != null && origin != null)
		{
			direction = ((Vector2)target.position - (Vector2)origin.position).normalized;
			currentDirection = direction;
		}

		Vector2 spawnPos = (Vector2)origin.position + currentDirection * spawnDistance + offset;
		float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
		var warningColor = new Color(1f, 0f, 0f, 0.3f * transparency);
		var activeColor = new Color(1f, 1f, 1f, transparency);

		GameObject warning = CreateMeleeVisual(shape, spawnPos, angle, width, height, arcAngle, warningColor);
		warning.layer = LayerMask.NameToLayer("Default");
		RegisterPatternObject(warning);

		// Warning phase
		float elapsed = 0f;
		while (elapsed < warningTime)
		{
			if (warning == null)
				yield break;

			elapsed += Time.deltaTime;

			// Update position and rotation to follow origin
			if (origin != null)
			{
				if (target != null)
				{
					Vector2 targetDirection = ((Vector2)target.position - (Vector2)origin.position).normalized;

					if (trackingLag > 0f)
					{
						float lerpSpeed = 1f / trackingLag;
						currentDirection = Vector2
							.Lerp(currentDirection, targetDirection, lerpSpeed * Time.deltaTime)
							.normalized;
					}
					else
					{
						currentDirection = targetDirection;
					}
				}

				spawnPos = (Vector2)origin.position + currentDirection * spawnDistance + offset;
				warning.transform.position = spawnPos;

				angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
				warning.transform.rotation = Quaternion.Euler(0, 0, angle);
			}

			yield return null;
		}

		// Lock direction for active phase
		Vector2 lockedDirection = currentDirection;
		float lockedAngle = angle;

		UnregisterPatternObject(warning);
		Destroy(warning);

		GameObject melee = CreateMeleeVisual(shape, spawnPos, lockedAngle, width, height, arcAngle, activeColor);
		RegisterPatternObject(melee);
		Collider2D collider = AddMeleeCollider(melee, shape, width, height, arcAngle);

		MeleeDamage meleeDamage = melee.AddComponent<MeleeDamage>();
		meleeDamage.Damage = damage;
		meleeDamage.HitOnce = hitOnce;
		meleeDamage.HitOnce = true;
		meleeDamage.HasKnockback = knockbackForce > 0f;
		meleeDamage.Force = knockbackForce;
		meleeDamage.Direction = knockbackFromCenter
			? KnockbackDirection.FromHitPosition
			: KnockbackDirection.FromSource;
		meleeDamage.SetSource(origin != null ? origin.gameObject : null);

		// Active phase - follow origin but DON'T rotate
		elapsed = 0f;
		while (elapsed < activeTime)
		{
			if (melee == null)
				yield break;

			elapsed += Time.deltaTime;

			if (origin != null)
			{
				spawnPos = (Vector2)origin.position + lockedDirection * spawnDistance + offset;
				melee.transform.position = spawnPos;
			}

			yield return null;
		}

		// End phase - fade out
		float fadeTime = 0.1f;
		SpriteRenderer sr = melee.GetComponent<SpriteRenderer>();
		float fadeElapsed = 0f;
		Quaternion lockedRotation = melee.transform.rotation;
		if (collider != null)
		{
			collider.enabled = false;
		}

		while (fadeElapsed < fadeTime)
		{
			if (melee == null)
				yield break;

			fadeElapsed += Time.deltaTime;

			if (sr != null)
			{
				Color c = sr.color;
				c.a = Mathf.Lerp(transparency, 0f, fadeElapsed / fadeTime);
				sr.color = c;
			}

			if (origin != null)
			{
				spawnPos = (Vector2)origin.position + lockedDirection * spawnDistance + offset;
				melee.transform.position = spawnPos;
				melee.transform.rotation = lockedRotation;
			}

			yield return null;
		}

		UnregisterPatternObject(melee);
		Destroy(melee);
	}

	private GameObject CreateMeleeVisual(
		int shape,
		Vector2 position,
		float angle,
		float width,
		float height,
		float arcAngle,
		Color color
	)
	{
		var melee = new GameObject("Melee");
		melee.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
		SpriteRenderer sr = melee.AddComponent<SpriteRenderer>();
		sr.sprite = CreateMeleeSprite(shape, width, height, arcAngle);
		sr.color = color;
		sr.sortingOrder = 10;

		return melee;
	}

	private Sprite CreateMeleeSprite(int shape, float width, float height, float arcAngle)
	{
		return shape switch
		{
			// Arc
			0 => CreateArcSprite(width, height, arcAngle),
			// Circle
			1 => CreateCircleSprite(width / 2f),
			// Rectangle
			2 => CreateRectangleSprite(width, height),
			// Cone
			3 => CreateConeSprite(width, height, arcAngle),
			_ => CreateArcSprite(width, height, arcAngle),
		};
	}

	private Sprite CreateArcSprite(float width, float height, float arcAngle)
	{
		int resolution = 128;
		var texture = new Texture2D(resolution, resolution);
		var pixels = new Color[resolution * resolution];

		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = Color.clear;
		}

		var center = new Vector2(0, resolution / 2f);
		float outerRadius = resolution * 0.95f;
		float innerRadius = outerRadius * 0.5f;
		float halfAngle = arcAngle / 2f * Mathf.Deg2Rad;

		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				var pos = new Vector2(x, y);
				Vector2 dir = pos - center;
				float dist = dir.magnitude;
				float angle = Mathf.Atan2(dir.y, dir.x);

				bool inRadius = dist >= innerRadius && dist <= outerRadius;
				bool inAngle = Mathf.Abs(angle) <= halfAngle;

				if (inRadius && inAngle)
				{
					pixels[y * resolution + x] = Color.white;
				}
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(
			texture,
			new Rect(0, 0, resolution, resolution),
			new Vector2(0, 0.5f),
			resolution / Mathf.Max(width, height)
		);
	}

	private Sprite CreateConeSprite(float width, float height, float coneAngle)
	{
		int resolution = 128;
		var texture = new Texture2D(resolution, resolution);
		var pixels = new Color[resolution * resolution];

		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = Color.clear;
		}

		var origin = new Vector2(0, resolution / 2f);
		float maxDist = resolution * 0.95f;
		float halfAngle = coneAngle / 2f * Mathf.Deg2Rad;

		for (int y = 0; y < resolution; y++)
		{
			for (int x = 0; x < resolution; x++)
			{
				var pos = new Vector2(x, y);
				Vector2 dir = pos - origin;
				float dist = dir.magnitude;
				float angle = Mathf.Atan2(dir.y, dir.x);

				bool inDist = dist <= maxDist;
				bool inAngle = Mathf.Abs(angle) <= halfAngle;

				if (inDist && inAngle)
				{
					pixels[y * resolution + x] = Color.white;
				}
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(
			texture,
			new Rect(0, 0, resolution, resolution),
			new Vector2(0, 0.5f),
			resolution / Mathf.Max(width, height)
		);
	}

	private Collider2D AddMeleeCollider(GameObject melee, int shape, float width, float height, float arcAngle)
	{
		PolygonCollider2D polyCol = melee.AddComponent<PolygonCollider2D>();
		polyCol.isTrigger = true;

		// Generate polygon points based on shape
		Vector2[] points = shape switch
		{
			// Arc
			0 => GenerateArcPolygon(width, height, arcAngle),
			// Circle
			1 => GenerateCirclePolygon(width / 2f, 16),
			// Rectangle
			2 => GenerateRectanglePolygon(width, height),
			// Cone
			3 => GenerateConePolygon(width, height, arcAngle),
			_ => GenerateArcPolygon(width, height, arcAngle),
		};
		polyCol.SetPath(0, points);
		return polyCol;
	}

	private Vector2[] GenerateArcPolygon(float width, float height, float arcAngle)
	{
		int segments = 12;
		float halfAngle = arcAngle / 2f * Mathf.Deg2Rad;

		// Match sprite: outer is 0.95 of size, inner is 0.5 of outer
		float size = Mathf.Max(width, height);
		float outerRadius = size * 0.95f;
		float innerRadius = outerRadius * 0.5f;

		var points = new List<Vector2>();

		// Outer arc (from -halfAngle to +halfAngle)
		for (int i = 0; i <= segments; i++)
		{
			float t = (float)i / segments;
			float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
			points.Add(new Vector2(Mathf.Cos(angle) * outerRadius, Mathf.Sin(angle) * outerRadius));
		}

		// Inner arc (from +halfAngle back to -halfAngle)
		for (int i = segments; i >= 0; i--)
		{
			float t = (float)i / segments;
			float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
			points.Add(new Vector2(Mathf.Cos(angle) * innerRadius, Mathf.Sin(angle) * innerRadius));
		}

		return points.ToArray();
	}

	private Vector2[] GenerateCirclePolygon(float radius, int segments)
	{
		var points = new Vector2[segments];

		float actualRadius = radius * 0.5f;

		for (int i = 0; i < segments; i++)
		{
			float angle = (float)i / segments * Mathf.PI * 2f;
			points[i] = new Vector2(Mathf.Cos(angle) * actualRadius, Mathf.Sin(angle) * actualRadius);
		}

		return points;
	}

	private Vector2[] GenerateRectanglePolygon(float width, float height)
	{
		float halfHeight = height / 2f;
		return new Vector2[]
		{
			new Vector2(0, -halfHeight),
			new Vector2(width, -halfHeight),
			new Vector2(width, halfHeight),
			new Vector2(0, halfHeight),
		};
	}

	private Vector2[] GenerateConePolygon(float width, float height, float coneAngle)
	{
		int segments = 12;
		float halfAngle = coneAngle / 2f * Mathf.Deg2Rad;

		// Match sprite: cone uses 0.95 factor
		float size = Mathf.Max(width, height);
		float radius = size * 0.95f;

		var points = new List<Vector2>
		{
			// Start at origin
			Vector2.zero,
		};

		// Arc from -halfAngle to +halfAngle
		for (int i = 0; i <= segments; i++)
		{
			float t = (float)i / segments;
			float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
			points.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
		}

		return points.ToArray();
	}

	/// <summary>
	/// Shoots a cluster bullet that explodes into multiple bullets after a fuse time.
	/// </summary>
	public void ShootCluster(
		float initialSpeed,
		float initialDamage,
		float fuseTime,
		int clusterCount,
		float clusterSpeed,
		float clusterDamage,
		float spreadAngle,
		float clusterLifetime,
		float initialBulletSize,
		bool clusterContinuesDirection,
		Transform origin,
		Transform target = null
	)
	{
		Vector2 direction;
		if (target != null)
		{
			direction = ((Vector2)target.position - (Vector2)origin.position).normalized;
		}
		else
		{
			direction = Vector2.right;
		}

		StartCoroutine(
			ClusterCoroutine(
				origin.position,
				direction,
				initialSpeed,
				initialDamage,
				fuseTime,
				clusterCount,
				clusterSpeed,
				clusterDamage,
				spreadAngle,
				clusterLifetime,
				initialBulletSize,
				clusterContinuesDirection,
				target
			)
		);
	}

	private IEnumerator ClusterCoroutine(
		Vector2 startPosition,
		Vector2 initialDirection,
		float initialSpeed,
		float initialDamage,
		float fuseTime,
		int clusterCount,
		float clusterSpeed,
		float clusterDamage,
		float spreadAngle,
		float clusterLifetime,
		float initialBulletSize,
		bool clusterContinuesDirection,
		Transform target
	)
	{
		// Create initial cluster bullet
		GameObject clusterBullet = CreateClusterBullet(startPosition, initialBulletSize, initialDamage);
		RegisterPatternObject(clusterBullet);

		ClusterTrigger trigger = clusterBullet.GetComponent<ClusterTrigger>();

		Vector2 position = startPosition;
		Vector2 velocity = initialDirection * initialSpeed;
		Vector2 travelDirection = initialDirection;

		float elapsed = 0f;
		while (elapsed < fuseTime)
		{
			if (clusterBullet == null)
			{
				yield break;
			}

			// Check for early collision
			if (trigger != null && trigger.HasHit)
			{
				break;
			}
			elapsed += Time.deltaTime;
			position += velocity * Time.deltaTime;
			clusterBullet.transform.position = position;

			yield return null;
		}

		Vector2 explosionPos = clusterBullet != null ? (Vector2)clusterBullet.transform.position : position;
		if (clusterBullet != null)
		{
			UnregisterPatternObject(clusterBullet);
			Destroy(clusterBullet);
		}

		// Determine cluster direction
		Vector2 clusterDirection;
		if (clusterContinuesDirection)
		{
			clusterDirection = travelDirection;
		}
		else if (target != null)
		{
			clusterDirection = ((Vector2)target.position - explosionPos).normalized;
		}
		else
		{
			clusterDirection = travelDirection;
		}

		float halfSpread = spreadAngle / 2f;

		for (int i = 0; i < clusterCount; i++)
		{
			float angle;
			if (clusterCount == 1)
			{
				angle = 0f;
			}
			else
			{
				float t = (float)i / (clusterCount - 1);
				angle = Mathf.Lerp(-halfSpread, halfSpread, t);
			}

			Vector2 bulletDir = RotateVector(clusterDirection, angle);
			SpawnClusterBullet(explosionPos, bulletDir * clusterSpeed, clusterDamage, clusterLifetime);
		}
	}

	private GameObject CreateClusterBullet(Vector2 position, float size, float damage)
	{
		var bullet = new GameObject("ClusterBullet");
		bullet.transform.position = position;

		SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
		sr.sprite = CreateCircleSprite(size);
		sr.color = Color.white;
		sr.sortingOrder = 10;
		CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = size / 2f;
		ClusterTrigger trigger = bullet.AddComponent<ClusterTrigger>();
		trigger.Damage = damage;

		return bullet;
	}

	private void SpawnClusterBullet(Vector2 position, Vector2 velocity, float damage, float lifetime)
	{
		SpawnBullet(position, velocity, damage, lifetime);
	}
}
