using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "Gameplay/Pattern")]
public class PatternSO : ScriptableObject
{
	[Header("Bullet Type")]
	[SerializeField]
	[Required]
	private BulletSO _bulletSO;

	[SerializeField]
	[Required]
	private BulletBehavior _behavior;

	[Header("Spread Settings")]
	[Tooltip("Total number of bullets to fire")]
	[SerializeField]
	[Min(1)]
	private int _bulletCount = 1;

	[Tooltip("The total arc in degrees (e.g. 360 for a full circle)")]
	[SerializeField]
	[Range(0, 360)]
	private float _spreadAngle = 360f;

	[Tooltip("Toggle to aim towards the player's direction")]
	[SerializeField]
	private bool _towardsPlayer = true;

	[Tooltip("Direction to aim bullets (0 = Right, 90 = Up)")]
	[HideIf("_towardsPlayer")]
	[Range(0, 360)]
	[SerializeField]
	private float _direction = 270f;

	[SerializeField]
	private float _baseSpeed = 5f;

	[Header("Behavior Settings")]
	[SerializeField]
	[Tooltip("Rotate bullet towards the velocity direction, disable for performance")]
	private bool _rotateTowardsDirection = false;

	[SerializeField]
	[Tooltip("The duration of the bullet")]
	private float _lifeTime = 8f;

	[ShowIf("_behavior", BulletBehavior.SineWave)]
	[SerializeField]
	private float _sineAmplitude = 1f;

	[ShowIf("_behavior", BulletBehavior.SineWave)]
	[SerializeField]
	private float _sineFrequency = 5f;

	[ShowIf("_behavior", BulletBehavior.Following)]
	[SerializeField]
	private float _trackingStrength = 3f;

	[ShowIf("_behavior", BulletBehavior.Steering)]
	[SerializeField]
	[Range(0f, 2f)]
	[Tooltip("Maximum steering force magnitude (higher = tighter turns)")]
	private float _maxSteerForce = 0.5f;

	[ShowIf("_behavior", BulletBehavior.Homing)]
	[SerializeField]
	[Tooltip("Maximum angular velocity, makes the bullet turn sharper")]
	private float _maxTurnRate = 180f;

	[ShowIf("_behavior", BulletBehavior.Homing)]
	[SerializeField]
	[Tooltip("Angular acceleration, makes the bullet turn faster")]
	private float _turnAcceleration = 360f;

	public BulletSO BulletSO => _bulletSO;
	public BulletBehavior Behavior => _behavior;
	public int BulletCount => _bulletCount;
	public float SpreadAngle => _spreadAngle;
	public bool TowardsPlayer => _towardsPlayer;
	public float Direction => _direction;
	public float BaseSpeed => _baseSpeed;
	public bool RotateTowardsDirection => _rotateTowardsDirection;
	public float MaxLifeTime => _lifeTime;
	public float SineAmplitude => _sineAmplitude;
	public float SineFrequency => _sineFrequency;
	public float TrackingStrength => _trackingStrength;
	public float MaxSteerForce => _maxSteerForce;
	public float MaxTurnRate => _maxTurnRate;
	public float TurnAcceleration => _turnAcceleration;
}
