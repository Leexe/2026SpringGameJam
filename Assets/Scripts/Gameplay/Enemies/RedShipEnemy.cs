using System.Collections.Generic;
using UnityEngine;

public class RedShipEnemy : MonoBehaviour
{
	[SerializeField]
	private List<PatternSO> _patterns;

	[SerializeField]
	private List<Transform> _shootPoints;

	[SerializeField]
	[Tooltip("How long it takes to shoot a shot in seconds")]
	private float _fireRate = 1f;

	[SerializeField]
	private bool _enableShooting = true;

	private float _fireRateTimer;
	private int _patternIndex;
	private int _shootPointIndex;

	private void Update()
	{
		if (_enableShooting)
		{
			if (_fireRateTimer >= _fireRate)
			{
				BulletManager.Instance.FirePattern(_patterns[_patternIndex], _shootPoints[_shootPointIndex].position);
				_patternIndex = (_patternIndex + 1) % _patterns.Count;
				_shootPointIndex = (_shootPointIndex + 1) % _shootPoints.Count;
				_fireRateTimer -= _fireRate;
			}
			_fireRateTimer += Time.deltaTime;
		}
	}
}
