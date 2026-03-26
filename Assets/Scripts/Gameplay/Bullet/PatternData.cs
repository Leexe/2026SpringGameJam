using UnityEngine;

[CreateAssetMenu(fileName = "NewPatternData", menuName = "Danmaku/Pattern Data")]
public class PatternData : ScriptableObject
{
	#region Fields

	[Header("Bullet Type")]
	[SerializeField]
	private BulletData _bulletType;

	[SerializeField]
	private BulletBehavior _behavior;

	[Header("Spread Settings")]
	[Tooltip("Total number of bullets to fire")]
	[SerializeField]
	private int _bulletCount = 1;

	[Tooltip("The total arc in degrees (e.g. 360 for a full circle)")]
	[SerializeField]
	private float _spreadAngle = 360f;

	[SerializeField]
	private float _baseSpeed = 5f;

	[Header("Behavior Settings")]
	[SerializeField]
	private float _sineAmplitude = 1f;

	[SerializeField]
	private float _sineFrequency = 5f;

	#endregion

	#region Properties

	public BulletData BulletType => _bulletType;
	public BulletBehavior Behavior => _behavior;
	public int BulletCount => _bulletCount;
	public float SpreadAngle => _spreadAngle;
	public float BaseSpeed => _baseSpeed;
	public float SineAmplitude => _sineAmplitude;
	public float SineFrequency => _sineFrequency;

	#endregion
}
