using Stats;
using UnityEngine;

public class CursorMovementController : MonoBehaviour
{
	[Header("Testing")]
	[SerializeField]
	private bool _readsInput = true;

	[Header("References")]
	[SerializeField]
	private Rigidbody2D _rb;

	[SerializeField]
	private Transform _fish;

	[SerializeField]
	private SpriteRenderer _sprite;

	[Header("Movement")]
	[SerializeField]
	[Tooltip("Drag experienced while inertial")]
	private float _drag = 0.8f;

	[SerializeField]
	[Tooltip("Below this speed, the player starts regaining control")]
	private float _inertiaRecoverySpeedThreshold = 0.4f;

	[SerializeField]
	[Tooltip("Rate at which player regains control")]
	private float _inertiaRecoveryRate = 1f;

	[Header("Test Scene Only")]
	[SerializeField]
	[Tooltip("How fast the player moves around")]
	private float _baseMoveSpeed = 3.5f;

	[SerializeField]
	[Tooltip("How fast the player moves around while reeling")]
	private float _reelingBaseMoveSpeed = 1f;

	public Vector2 MovementVector { get; private set; }
	public Vector2 LastNonZeroMovementVector { get; private set; }
	public bool AnchorHeld { get; private set; }

	public float MovementSpeedMult { get; private set; }
	public bool ForceInertial { get; private set; }
	public bool IsInertial { get; private set; }
	public float Inertia { get; private set; }
	public bool IsAirborne { get; private set; }

	public MovementState State { get; private set; }

	public Rigidbody2D Rb => _rb;
	public float BaseMoveSpeed => _movespeed;

	// public bool AbilityHeld { get; private set; }

	private float _movespeed;
	private float _reelingSpeed;

	private void Start()
	{
		_movespeed = _baseMoveSpeed;
		_reelingSpeed = _reelingBaseMoveSpeed;
	}

	private void OnEnable()
	{
		if (_readsInput)
		{
			InputManager.Instance.OnMovement.AddListener(OnMovement);
			InputManager.Instance.OnAnchorPerformed.AddListener(OnAnchorPerformed);
			InputManager.Instance.OnAnchorReleased.AddListener(OnAnchorReleased);
			// InputManager.Instance.OnAbilityPerformed.AddListener(OnAbilityPerformed);
			// InputManager.Instance.OnAbilityReleased.AddListener(OnAbilityReleased);
		}

		Init();
	}

	private void OnDisable()
	{
		if (_readsInput && InputManager.Instance)
		{
			InputManager.Instance.OnMovement.RemoveListener(OnMovement);
			InputManager.Instance.OnAnchorPerformed.RemoveListener(OnAnchorPerformed);
			InputManager.Instance.OnAnchorReleased.RemoveListener(OnAnchorReleased);
			// InputManager.Instance.OnAbilityPerformed.RemoveListener(OnAbilityPerformed);
			// InputManager.Instance.OnAbilityReleased.RemoveListener(OnAbilityReleased);
		}
	}

	private void FixedUpdate()
	{
		// HandleAbility();
		HandleMovement();
	}

	private void Init()
	{
		State = MovementState.Controlled;

		MovementVector = Vector2.zero;
		MovementSpeedMult = 1f;
		LastNonZeroMovementVector = Vector2.up;
		AnchorHeld = false;
		// AbilityHeld = false;
		Inertia = 0.2f;
		ForceInertial = false;

		IsAirborne = false;
	}

	// Updates movement vector with input
	private void OnMovement(Vector2 movement)
	{
		MovementVector = movement;
		if (movement != Vector2.zero)
		{
			LastNonZeroMovementVector = movement;
		}
	}

	private void OnAnchorPerformed()
	{
		AnchorHeld = true;
	}

	private void OnAnchorReleased()
	{
		AnchorHeld = false;
	}

	// private void OnAbilityPerformed()
	// {
	// 	AbilityHeld = true;
	// }

	// private void OnAbilityReleased()
	// {
	// 	AbilityHeld = false;
	// 	_inertia *= 0.5f; // hack
	// }

	public void Grab(Transform parent)
	{
		transform.SetParent(parent);
		// note that local Z must also be set to 0 for rotating parents
		transform.localPosition = new(0f, 0f, 0f);
		State = MovementState.Grabbed;
	}

	public void UnGrab(MovementState resultingState = MovementState.Controlled)
	{
		transform.SetParent(null);
		State = resultingState;
	}

	public void KinematicMode()
	{
		State = MovementState.Kinematic;
	}

	public void ControlledMode()
	{
		if (State == MovementState.Grabbed)
		{
			UnGrab();
		}
		State = MovementState.Controlled;
	}

	public void SetMovementSpeedMult(float mult)
	{
		MovementSpeedMult = mult;
	}

	/// <summary>
	/// Sets inertia, but does nothing if current inertia is higher (unless <c>overwrite</c> is true)
	/// </summary>
	public void SetInertia(float inertia, bool overwrite = false)
	{
		Inertia = overwrite ? inertia : Mathf.Max(Inertia, inertia);
	}

	/// <summary>
	/// Sets inertia, and keeps it at that value
	/// </summary>
	public void PinInertia(float inertia)
	{
		Inertia = inertia;
		ForceInertial = true;
	}

	public void UnpinInertia(float restoredInertia = -1f)
	{
		if (restoredInertia >= 0f)
		{
			Inertia = restoredInertia;
		}
		ForceInertial = false;
	}

	public void Launch(Vector2 vel, bool additive = false)
	{
		if (State != MovementState.Controlled)
		{
			return;
		}

		Inertia = 0.1f;
		IsInertial = true;

		if (additive)
		{
			// even if additive, stacked knockback will NOT increase the magnitude of your speed.
			// this prevents getting launched to space by simultaneous hits.

			float maxMag = Mathf.Max(vel.magnitude, _rb.linearVelocity.magnitude);
			var newVel = Vector2.ClampMagnitude(_rb.linearVelocity + vel, maxMag);
			_rb.linearVelocity = newVel;
		}
		else
		{
			_rb.linearVelocity = vel;
		}
	}

	// private void HandleAbility()
	// {
	// 	if (_grabbed)
	// 	{
	// 		return;
	// 	}

	// 	if (AbilityHeld)
	// 	{
	// 		_forceInertial = true;
	// 		_inertia = 0.2f;
	// 		Vector2 offset = transform.position - _fish.position;

	// 		Vector2 currentVel = _rb.linearVelocity;
	// 		currentVel += 20f * Time.fixedDeltaTime * -offset.normalized;
	// 		_rb.linearVelocity = currentVel;
	// 	}
	// 	else
	// 	{
	// 		_forceInertial = false;
	// 	}
	// }

	private void HandleMovement()
	{
		switch (State)
		{
			case MovementState.Controlled:
			{
				// we need the if-statements, because repeatedly setting these breaks rb interpolation.
				if (!_rb.simulated)
				{
					_rb.simulated = true;
				}
				if (_rb.bodyType != RigidbodyType2D.Dynamic)
				{
					_rb.bodyType = RigidbodyType2D.Dynamic;
				}
				HandleActiveMovement();
				break;
			}
			case MovementState.Kinematic:
			{
				if (!_rb.simulated)
				{
					_rb.simulated = true;
				}
				if (_rb.bodyType != RigidbodyType2D.Kinematic)
				{
					_rb.bodyType = RigidbodyType2D.Kinematic;
				}
				break;
			}
			case MovementState.Grabbed:
			{
				if (_rb.simulated)
				{
					_rb.simulated = false;
				}
				if (_rb.bodyType != RigidbodyType2D.Kinematic)
				{
					_rb.bodyType = RigidbodyType2D.Kinematic;
				}
				// reset position and rotation when grabbed;
				transform.rotation = Quaternion.identity;
				transform.localPosition = new(0f, 0f, 0f);
				break;
			}
		}
	}

	private void HandleActiveMovement()
	{
		// compute speed, accel, inertia
		if (Inertia < 0f)
		{
			Debug.LogError("Negative Inertia");
			return;
		}

		Vector2 currentVel = _rb.linearVelocity;
		float currentSpeed = currentVel.magnitude;

		// airborne logic
		bool airborne = _rb.position.y > 0f;
		if (airborne)
		{
			IsInertial = true;
			Inertia = Mathf.Max(0.3f, Inertia);
			currentVel += Physics2D.gravity * Time.fixedDeltaTime;
		}
		else
		{
			if (!IsAirborne)
			{
				// restore some control upon re-entering water.
				Inertia *= 0.6f;
			}
		}
		IsAirborne = airborne;

		// inertia recovery
		if (!ForceInertial && (currentSpeed < _inertiaRecoverySpeedThreshold || !IsInertial))
		{
			IsInertial = false;
			Inertia = Mathf.MoveTowards(Inertia, 0f, _inertiaRecoveryRate * Time.fixedDeltaTime);
		}

		_rb.linearDamping = IsAirborne && Inertia > 0.01f ? _drag : 0f;

		float speed = (AnchorHeld ? _reelingSpeed : _movespeed) * MovementSpeedMult;
		float accel = 1f / (Inertia + 0.001f);

		// apply motion
		Vector2 targetVel = speed * MovementVector.normalized;

		_rb.linearVelocity = Vector2.MoveTowards(currentVel, targetVel, accel * Time.fixedDeltaTime);
	}

	/// <summary>
	/// Snaps the position of the player to the position
	/// </summary>
	public void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	public void SetMoveSpeed(float speed)
	{
		_movespeed = speed;
	}

	public void SetReelSpeed(float speed)
	{
		_reelingSpeed = speed;
	}

	public enum MovementState
	{
		Controlled, // moving according to physics and user input
		Kinematic, // does not move, but can be moved by external scripts
		Grabbed,
	}
}
