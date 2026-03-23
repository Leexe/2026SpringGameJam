using System.Collections;
using UnityEngine;

public class Laser : BulletPattern
{
	[SerializeField]
	private GameObject _laserPrefab;

	[Header("Laser Settings")]
	[SerializeField]
	private float _length = 10f;

	[SerializeField]
	private float _width = 0.5f;

	[SerializeField]
	private float _warningTime = 1f;

	[SerializeField]
	private float _activeTime = 2f;

	[SerializeField]
	private float _damagePerSecond = 20f;

	[SerializeField]
	private bool _aimAtPlayer = true;

	[SerializeField]
	private Vector2 _staticDirection = Vector2.right;

	[SerializeField]
	private bool _trackTarget = true;

	[SerializeField]
	private float _turnRate = 5f;

	[Header("Animation")]
	[SerializeField]
	private float _formDuration = 0.3f;

	[SerializeField]
	private float _breakDuration = 0.3f;

	private Transform _originTransform;

	public void SetOriginTransform(Transform origin)
	{
		_originTransform = origin;
	}

	public override void Fire(Vector2 origin, Vector2? playerPos = null)
	{
		Vector2 direction = _staticDirection.normalized;

		if (_aimAtPlayer && playerPos != null)
		{
			direction = (playerPos.Value - origin).normalized;
		}

		StartCoroutine(LaserCoroutine(origin, direction, playerPos));
	}

	private IEnumerator LaserCoroutine(Vector2 initialOrigin, Vector2 initialDirection, Vector3? playerPos)
	{
		// Instantiate laser prefab
		GameObject laserObj = Instantiate(_laserPrefab, initialOrigin, Quaternion.identity);
		LaserVisual laserVisual = laserObj.GetComponent<LaserVisual>();

		if (laserVisual == null)
		{
			Debug.LogError("Laser prefab missing LaserVisual component!");
			Destroy(laserObj);
			yield break;
		}

		laserVisual.Initialize(_length, _width, _damagePerSecond);
		laserVisual.SetDirection(initialDirection);
		laserVisual.SetWarningState(true);

		Vector2 currentDirection = initialDirection;

		// Warning phase
		laserVisual.PlayForm(_formDuration);

		float elapsed = 0f;
		while (elapsed < _warningTime)
		{
			if (laserObj == null)
				yield break;

			elapsed += Time.deltaTime;

			Vector2 currentOrigin = _originTransform != null ? (Vector2)_originTransform.position : initialOrigin;
			laserObj.transform.position = currentOrigin;

			// Track target
			if (_trackTarget && _aimAtPlayer)
			{
				var player = GameObject.FindGameObjectWithTag("Player");
				if (player != null)
				{
					Vector2 targetDirection = ((Vector2)player.transform.position - currentOrigin).normalized;
					currentDirection = RotateTowards(currentDirection, targetDirection, _turnRate * Time.deltaTime);
					laserVisual.SetDirection(currentDirection);
				}
			}

			yield return null;
		}

		laserVisual.SetWarningState(false);

		// Active phase
		elapsed = 0f;
		while (elapsed < _activeTime)
		{
			if (laserObj == null)
				yield break;

			elapsed += Time.deltaTime;
			Vector2 currentOrigin = _originTransform != null ? (Vector2)_originTransform.position : initialOrigin;
			laserObj.transform.position = currentOrigin;

			// Track target
			if (_trackTarget && _aimAtPlayer)
			{
				var player = GameObject.FindGameObjectWithTag("Player");
				if (player != null)
				{
					Vector2 targetDirection = ((Vector2)player.transform.position - currentOrigin).normalized;
					currentDirection = RotateTowards(currentDirection, targetDirection, _turnRate * Time.deltaTime);
					laserVisual.SetDirection(currentDirection);
				}
			}

			yield return null;
		}

		// Break phase
		laserVisual.PlayBreak(_breakDuration);
		yield return new WaitForSeconds(_breakDuration);

		if (laserObj != null)
		{
			Destroy(laserObj);
		}
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
}
