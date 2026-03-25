using System;
using Sirenix.OdinInspector;
using Unity.AppUI.UI;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[field:SerializeField]
	public Rigidbody2D Rb { get; private set; }

	[Header("Parameters - Movement")]
	[SerializeField]
	private float _baseMoveSpeed = 7f;
	[SerializeField]
	private float _repairSpeedMult = 0.5f;

	[Header("Parameters - Repair")]
	[SerializeField, Tooltip("Time it takes to repair from _startingRepairValue to 1")]
	private float _repairTimeNeeded = 12f;
	[SerializeField, Tooltip("Starting repair value")]
	private float _startingRepairValue = 0.3f;
	[SerializeField, Tooltip("Time it takes for Instability to go from 0 to 1")]
	private float _timeUntilDeath = 30f;
	[SerializeField, Tooltip("Time it takes for Instability to go from 1 to 0, when you win")]
	private float _instabilityFadeTime = 3f;

	/** Fields **/

	public event Action OnDie;
	public event Action<float> OnRepairProgressChanged;
	public event Action<float> OnInstabilityProgressChanged;

	private bool _isRepairInputHeld;
	private Vector2 _movementInput;

	public bool IsAlive { get; private set; }

	// if instability > repair, lose. repair gets a head start (it doesn't start at 0)
	public float RepairProgress { get; private set; }
	public float InstabilityProgress { get; private set; }

	/** Unity Messages **/

	private void OnEnable()
	{
		InputManager.Instance.OnMovement.AddListener(HandleMovement);
		InputManager.Instance.OnAnchorPerformed.AddListener(HandleAnchorPerformed);
		InputManager.Instance.OnAnchorReleased.AddListener(HandleAnchorReleased);

		_isRepairInputHeld = false;
		IsAlive = true;

		RepairProgress = 1f;
		InstabilityProgress = 0f;
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
		Die(true);
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

	[Button]
	public void ResetRepairProgress()
	{
		RepairProgress = _startingRepairValue;
		InstabilityProgress = 0f;

		OnRepairProgressChanged?.Invoke(RepairProgress);
		OnInstabilityProgressChanged?.Invoke(InstabilityProgress);
	}

	/** Private Methods **/

	private void HandleMovementPhysics()
	{
		float speed = _baseMoveSpeed * (_isRepairInputHeld ? _repairSpeedMult : 1f);
		Rb.linearVelocity = speed * Vector2.ClampMagnitude(_movementInput, 1f);
	}

	private void HandleRepairs(float dt)
	{
		float oldProg = RepairProgress;
		float oldInstProg = InstabilityProgress;

		if (_isRepairInputHeld)
		{
			float repairRate = (1f - _startingRepairValue) / _repairTimeNeeded;
			RepairProgress = Mathf.Clamp01(RepairProgress + (repairRate * dt));
		}

		float instabilityRate =  1f / (RepairProgress >= 1f ? -_instabilityFadeTime : _timeUntilDeath);
		InstabilityProgress += instabilityRate * dt;

		if (RepairProgress < InstabilityProgress)
		{
			Die(false);
		}

		if (oldProg != RepairProgress)
		{
			OnRepairProgressChanged?.Invoke(RepairProgress);
		}
		if (oldInstProg != InstabilityProgress)
		{
			OnInstabilityProgressChanged?.Invoke(InstabilityProgress);
		}
	}

	private void Die(bool isFromHit)
	{
		if (!IsAlive)
		{
			return;
		}
		IsAlive = false;

		OnDie?.Invoke();
		// TBD
	}
}
