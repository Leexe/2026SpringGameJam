using System;
using Animancer;
using Sirenix.OdinInspector;
using Unity.AppUI.UI;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[field: SerializeField]
	public Rigidbody2D Rb { get; private set; }

	public Vector2 MovementInput => _movementInput;

	[Header("Parameters - Movement")]
	[SerializeField]
	private float _baseMoveSpeed = 7f;

	[SerializeField]
	private float _repairSpeedMult = 0.5f;

	[Header("Parameters - Repair")]
	[field: SerializeField, Tooltip("Time it takes to finish repairs")]
	public float SecondsToRepair { get; private set; } = 12f;

	[field: SerializeField, Tooltip("Time it takes for Instability to go from 0 to 1")]
	public float SecondsToDie { get; private set; } = 30f;

	[Header("Layers")]
	[SerializeField]
	private LayerMask _enemyLayer;

	/** Fields **/

	public event Action<bool> OnDie; // isFromHit
	public event Action OnDeathAnimationFinished;
	public event Action<float> OnRepairProgressChanged; // secs, max
	public event Action<float> OnInstabilityProgressChanged; // secs, max;
	public event Action OnRepairFilledUp;

	private bool _isRepairInputHeld;
	private Vector2 _movementInput;

	public bool DisableInstability { get; set; } = false;

	// if instability > repair, lose. repair gets a head start (it doesn't start at 0)
	public bool IsAlive { get; private set; }
	public bool CanRepair { get; private set; } = true;
	public bool IsBuffering { get; private set; }
	private float _bufferDuration;
	private float _bufferTimer;
	private float _startBufferRepair;
	private float _startBufferDie;

	private float _defaultSecondsToRepair;
	private float _defaultSecondsToDie;

	public float RepairSecondsLeft { get; private set; }
	public float DieSecondsLeft { get; private set; }

	private Vector2 _startPosition;

	/** Unity Messages **/

	private void OnEnable()
	{
		InputManager.Instance.OnMovement.AddListener(HandleMovement);
		InputManager.Instance.OnAnchorPerformed.AddListener(HandleAnchorPerformed);
		InputManager.Instance.OnAnchorReleased.AddListener(HandleAnchorReleased);

		_isRepairInputHeld = false;
		IsAlive = true;

		RepairSecondsLeft = 1f;
		DieSecondsLeft = 0f;
	}

	private void OnDisable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement.RemoveListener(HandleMovement);
			InputManager.Instance.OnAnchorPerformed.RemoveListener(HandleAnchorPerformed);
			InputManager.Instance.OnAnchorReleased.RemoveListener(HandleAnchorReleased);
		}
	}

	private void Start()
	{
		_startPosition = transform.position;
		_defaultSecondsToRepair = SecondsToRepair;
		_defaultSecondsToDie = SecondsToDie;
	}

	private void FixedUpdate()
	{
		if (IsAlive)
		{
			HandleMovementPhysics();
			HandleRepairs(Time.fixedDeltaTime);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
		{
			Die(true);
		}
	}

	/** Event Handlers **/

	private void HandleMovement(Vector2 movement)
	{
		_movementInput = movement;
	}

	private void HandleAnchorPerformed()
	{
		_isRepairInputHeld = true;
	}

	private void HandleAnchorReleased()
	{
		_isRepairInputHeld = false;
	}

	/** Public Methods **/

	public void TriggerDeathAnimationFinished()
	{
		OnDeathAnimationFinished?.Invoke();
	}

	[Button]
	public void ResetRepairProgress(float secondsToRepair, float secondsToDie)
	{
		SecondsToRepair = secondsToRepair;
		SecondsToDie = secondsToDie;

		RepairSecondsLeft = SecondsToRepair;
		DieSecondsLeft = SecondsToDie;

		IsBuffering = false;
		CanRepair = true;

		OnRepairProgressChanged?.Invoke(RepairSecondsLeft);
		OnInstabilityProgressChanged?.Invoke(DieSecondsLeft);
	}

	public void StartBuffer(float fillDuration)
	{
		SecondsToRepair = _defaultSecondsToRepair;
		SecondsToDie = _defaultSecondsToDie;

		CanRepair = false;
		IsBuffering = true;
		_bufferDuration = fillDuration;
		_bufferTimer = 0f;
		_startBufferRepair = RepairSecondsLeft;
		_startBufferDie = DieSecondsLeft;
	}

	public void ResetRepairProgress()
	{
		ResetRepairProgress(_defaultSecondsToRepair, _defaultSecondsToDie);
	}

	public void Respawn()
	{
		transform.position = _startPosition;
		Rb.linearVelocity = Vector2.zero;

		_isRepairInputHeld = false;
		IsAlive = true;

		RepairSecondsLeft = 1f;
		DieSecondsLeft = 0f;
	}

	/** Private Methods **/

	private void HandleMovementPhysics()
	{
		float speed = _baseMoveSpeed * (_isRepairInputHeld ? _repairSpeedMult : 1f);
		Rb.linearVelocity = speed * Vector2.ClampMagnitude(_movementInput, 1f);
	}

	private void HandleRepairs(float dt)
	{
		float oldProg = RepairSecondsLeft;
		float oldInstProg = DieSecondsLeft;

		if (IsBuffering)
		{
			_bufferTimer += dt;
			float t = _bufferDuration > 0f ? Mathf.Clamp01(_bufferTimer / _bufferDuration) : 1f;
			RepairSecondsLeft = Mathf.Lerp(_startBufferRepair, SecondsToRepair, t);
			DieSecondsLeft = Mathf.Lerp(_startBufferDie, SecondsToDie, t);

			if (t >= 1f)
			{
				IsBuffering = false;
				CanRepair = true;
				OnRepairFilledUp?.Invoke();
			}
		}
		else
		{
			if (_isRepairInputHeld && CanRepair)
			{
				RepairSecondsLeft = Mathf.Max(0f, RepairSecondsLeft - dt);
			}

			if (!DisableInstability && RepairSecondsLeft > 0f)
			{
				DieSecondsLeft = Mathf.Max(0f, DieSecondsLeft - dt);
			}

			if (DieSecondsLeft < RepairSecondsLeft)
			{
				Die(false);
			}
		}

		if (!Mathf.Approximately(oldProg, RepairSecondsLeft))
		{
			OnRepairProgressChanged?.Invoke(RepairSecondsLeft);
		}
		if (!Mathf.Approximately(oldInstProg, DieSecondsLeft))
		{
			OnInstabilityProgressChanged?.Invoke(DieSecondsLeft);
		}
	}

	private void Die(bool isFromHit)
	{
		if (!IsAlive)
		{
			return;
		}
		IsAlive = false;

		OnDie?.Invoke(isFromHit);

		DieSecondsLeft = 0f;
		OnInstabilityProgressChanged?.Invoke(DieSecondsLeft);

		Rb.linearVelocity = Vector2.zero;

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Explosion_Sfx);
	}

	public void DieFromHit()
	{
		Die(true);
	}
}
