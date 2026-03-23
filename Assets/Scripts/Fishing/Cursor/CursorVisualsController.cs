using UnityEngine;

// TODO: make this class :)
public class CursorVisualsController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private CursorController _cursor;

	[SerializeField]
	private SpriteRenderer _sprite;

	[Header("Visuals - Movement")]
	[SerializeField]
	[Tooltip("Idle sprite spin rate (deg/s)")]
	private float _idleSpinSpeed = 200f;

	[SerializeField]
	[Tooltip("Idle sprite spin rate (deg/s)")]
	private float _movingSpinSpeed = 500f;

	[SerializeField]
	[Tooltip("Idle sprite spin rate (deg/s)")]
	private float _reelingSpinSpeed = 1000f;

	[SerializeField]
	[Tooltip("Spin rate accel")]
	private float _spinRateAccel = 3000f;

	// definitely need to design this differently
	[Header("Visuals - Health")]
	[SerializeField]
	private Color _normalColor = Color.lightGray;

	[SerializeField]
	private Color _dyingColor = Color.red;

	[SerializeField]
	private Color _lowHpPulseColor = Color.crimson;

	[SerializeField]
	private float _lowHpPulseHpThreshold = 0.2f;

	[SerializeField]
	private float _lowHpPulseFreq = 10f;

	[SerializeField]
	private float _minimumDamageForPulse = 3f;

	[SerializeField]
	private float _damagePulseDuration = 0.1f;

	[SerializeField]
	private Color _damagePulseColor = Color.white;

	//

	private float _spinRate;
	private float _damagePulseTimer;

	/** Unity Messages **/

	private void OnEnable()
	{
		_spinRate = 0f;
		_damagePulseTimer = 0f;

		if (_cursor.Health != null)
		{
			_cursor.Damageable.OnHealthDamaged += HandleDamage;
		}
	}

	private void OnDisable()
	{
		if (_cursor != null && _cursor.Health != null)
		{
			_cursor.Damageable.OnHealthDamaged -= HandleDamage;
		}
	}

	private void Update()
	{
		HandleMovementVisuals();

		// TODO: visuals can be reworked to not update every frame
		HandleHealthVisuals();
	}

	/** Event Handlers **/

	private void HandleDamage(DamageData damageData)
	{
		if (damageData.Amount > _minimumDamageForPulse)
		{
			_damagePulseTimer = _damagePulseDuration;
		}
	}

	/** Core Visuals **/

	private void HandleMovementVisuals()
	{
		CursorMovementController movement = _cursor.Movement;

		bool moving =
			movement.MovementVector != Vector2.zero
			&& movement.State == CursorMovementController.MovementState.Controlled;

		// spin
		float targetSpinRate = movement.AnchorHeld ? _reelingSpinSpeed : (moving ? _movingSpinSpeed : _idleSpinSpeed);
		_spinRate = Mathf.MoveTowards(_spinRate, targetSpinRate, _spinRateAccel * Time.deltaTime);
		_sprite.transform.Rotate(0f, 0f, _spinRate * Time.deltaTime);
	}

	private void HandleHealthVisuals()
	{
		// all temp stuff here

		if (_damagePulseTimer > 0f)
		{
			_damagePulseTimer -= Time.deltaTime;
			_sprite.color = _damagePulseColor;
			return;
		}

		float normalizedHealth = _cursor.Health.GetNormalizedHealth;
		var spriteColor = Color.Lerp(_dyingColor, _normalColor, normalizedHealth);

		if (normalizedHealth < _lowHpPulseHpThreshold)
		{
			float lerp = (0.5f * Mathf.Sin(Time.time * _lowHpPulseFreq)) + 0.5f;
			spriteColor = Color.Lerp(spriteColor, _lowHpPulseColor, lerp);
		}

		_sprite.color = spriteColor;
	}
}
