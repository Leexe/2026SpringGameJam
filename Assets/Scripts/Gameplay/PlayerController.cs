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
	[field: SerializeField, Tooltip("Time it takes to finish repairs")]
	public float SecondsToRepair { get; private set; } = 12f;

	[field: SerializeField, Tooltip("Time it takes for Instability to go from 0 to 1")]
	public float SecondsToDie { get; private set; } = 30f;

	/** Fields **/

	public event Action OnDie;
	public event Action<float> OnRepairProgressChanged; // secs, max
	public event Action<float> OnInstabilityProgressChanged; // secs, max;

	private bool _isRepairInputHeld;
	private Vector2 _movementInput;
	private AnimancerState _animancerState;

	public bool DisableInstability { get; set; } = false;

	// if instability > repair, lose. repair gets a head start (it doesn't start at 0)
	public bool IsAlive { get; private set; }
	public float RepairSecondsLeft { get; private set; }
	public float DieSecondsLeft { get; private set; }

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
	public void ResetRepairProgress(float secondsToRepair, float secondsToDie)
	{
		SecondsToRepair = secondsToRepair;
		SecondsToDie = secondsToDie;

		RepairSecondsLeft = SecondsToRepair;
		DieSecondsLeft = SecondsToDie;

		OnRepairProgressChanged?.Invoke(RepairSecondsLeft);
		OnInstabilityProgressChanged?.Invoke(DieSecondsLeft);
	}

	public void ResetRepairProgress()
	{
		ResetRepairProgress(SecondsToRepair, SecondsToDie);
	}

	/** Private Methods **/

	private void HandleVisuals()
	{
		if (IsAlive)
		{
			bool thrust = _movementInput.y > 0f;

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
		float oldProg = RepairSecondsLeft;
		float oldInstProg = DieSecondsLeft;

		if (_isRepairInputHeld)
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

		if (oldProg != RepairSecondsLeft)
		{
			OnRepairProgressChanged?.Invoke(RepairSecondsLeft);
		}
		if (oldInstProg != DieSecondsLeft)
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

		OnDie?.Invoke();
		// TBD

		DieSecondsLeft = 0f;
		OnInstabilityProgressChanged?.Invoke(DieSecondsLeft);

		Rb.linearVelocity = Vector2.zero;
		_animancer.Play(_deathAnim);
	}
}
