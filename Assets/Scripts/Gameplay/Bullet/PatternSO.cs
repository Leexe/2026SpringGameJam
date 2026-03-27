using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "Gameplay/Pattern")]
public class PatternSO : ScriptableObject
{
	[Header("Bullet Type")]
	[SerializeField]
	private BulletSO _bulletSO;

	[SerializeField]
	private BulletBehavior _behavior;

	[Header("Spread Settings")]
	[Tooltip("Total number of bullets to fire")]
	[SerializeField]
	private int _bulletCount = 1;

	[Tooltip("The total arc in degrees (e.g. 360 for a full circle)")]
	[SerializeField]
	private float _spreadAngle = 360f;

	[Tooltip("Toggle to aim towards the player's direction")]
	[SerializeField]
	private bool _towardsPlayer = true;

	[Tooltip("Direction to aim bullets (0 = Right, 90 = Up)")]
	[ShowIf("_towardsPlayer")]
	[Range(0, 360)]
	[SerializeField]
	private float _direction = 270f;

	[SerializeField]
	private float _baseSpeed = 5f;

	[Header("Behavior Settings")]
	[SerializeField]
	[Tooltip("Rotate bullet towards the velocity direction, disable for performance")]
	private bool _rotateTowardsDirection = false;

	[ShowIf("_behavior", BulletBehavior.SineWave)]
	[SerializeField]
	private float _sineAmplitude = 1f;

	[ShowIf("_behavior", BulletBehavior.SineWave)]
	[SerializeField]
	private float _sineFrequency = 5f;

	[ShowIf("_behavior", BulletBehavior.Tracking)]
	[SerializeField]
	private float _trackingStrength = 3f;

	public BulletSO BulletSO => _bulletSO;
	public BulletBehavior Behavior => _behavior;
	public bool RotateTowardsDirection => _rotateTowardsDirection;
	public int BulletCount => _bulletCount;
	public float SpreadAngle => _spreadAngle;
	public float BaseSpeed => _baseSpeed;
	public float SineAmplitude => _sineAmplitude;
	public float SineFrequency => _sineFrequency;
	public float TrackingStrength => _trackingStrength;
}
