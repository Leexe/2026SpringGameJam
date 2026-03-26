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

	[SerializeField]
	private AnimancerComponent _animancer;

	[Header("Animations")]
	[SerializeField]
	private AnimationClip _idleThrustAnim;

	[SerializeField]
	private AnimationClip _leftThrustAnim;

	[SerializeField]
	private AnimationClip _rightThrustAnim;

	[SerializeField]
	private AnimationClip _idleNoThrustAnim;

	[SerializeField]
	private AnimationClip _leftNoThrustAnim;

	[SerializeField]
	private AnimationClip _rightNoThrustAnim;

	[SerializeField]
	private AnimationClip _deathAnim;

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
	private AnimancerState _animancerState;

	public bool DisableInstability { get; set; } = false;

	// if instability > repair, lose. repair gets a head start (it doesn't start at 0)
	public bool IsAlive { get; private set; }
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

	private void Update()
	{
		HandleVisuals();
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
	public void ResetRepairProgress(float to)
	{
		RepairProgress = to;
		InstabilityProgress = 0f;

		OnRepairProgressChanged?.Invoke(RepairProgress);
		OnInstabilityProgressChanged?.Invoke(InstabilityProgress);
	}

	public void ResetRepairProgress()
	{
		ResetRepairProgress(_startingRepairValue);
	}

	/** Private Methods **/

	private void HandleVisuals()
	{
		if (IsAlive)
		{
			bool thrust = !_isRepairInputHeld;

			AnimationClip animToPlay = thrust ? _idleThrustAnim : _idleNoThrustAnim;
			if (_movementInput.x < 0f)
			{
				animToPlay = thrust ? _leftThrustAnim : _leftNoThrustAnim;
			}
			else if (_movementInput.x > 0f)
			{
				animToPlay = thrust ? _rightThrustAnim : _rightNoThrustAnim;
			}

			if (_animancerState == null || _animancerState.Clip != animToPlay)
			{
				_animancerState = _animancer.Play(animToPlay);
			}
		}
	}

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

		float instabilityRate = 1f / (RepairProgress >= 1f ? -_instabilityFadeTime : _timeUntilDeath);
		if (DisableInstability)
		{
			instabilityRate = 0f;
		}
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

		InstabilityProgress = 1f;
		OnInstabilityProgressChanged?.Invoke(InstabilityProgress);

		Rb.linearVelocity = Vector2.zero;
		_animancer.Play(_deathAnim);
	}
}
