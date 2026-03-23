using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FishMovementController : AbstractFishMovementController
{
	[FoldoutGroup("Debug")]
	[SerializeField]
	private bool _takesInput;

	[FoldoutGroup("Debug")]
	[SerializeField]
	private float _inputMagnitude = 1f;

	[Header("References")]
	[SerializeField]
	private Transform _tiltParent;

	[Header("Movement")]
	[SerializeField]
	private float _acceleration = 2f;

	[SerializeField]
	[Tooltip("Speed is multiplied by this value upon jumping out of water. Prevents massive jumps.")]
	private float _waterBreakSpeedMultiplier = 0.7f;

	[SerializeField]
	private float _defaultIdleSwimSpeed = 0.5f;

	[Header("Visual")]
	[SerializeField]
	private float _tiltSpeed = 10000f;

	[SerializeField]
	private float _visualTiltLerpRate = 3f;

	// for debug functionality
	private bool _didRegisterInputHandler = false;

	// inputs/state vars of movement methods
	private float _idleSwimSpeed;
	private float _swimSpeed;
	public MovementMode MoveMode { get; private set; }
	private Vector2 _targetVelocity;
	private Vector2 _targetPosition;
	private Transform _targetTransform;
	private float _slowDownDistance;
	private float _stoppingDistance;

	// the final direction that the fish will try to move in
	public Vector2 MoveVelocity { get; private set; }

	private Quaternion _targetRot;
	private float _lastNonZeroSwimVelX;
	private float _visualTilt;
	private float _targetVisualTilt;

	private bool _airborne;

	public float VisualTilt => _targetVisualTilt;

	/** Unity Messages **/

	private void OnEnable()
	{
		if (_takesInput)
		{
			InputManager.Instance.OnMovement.AddListener(HandleMovementInput);
			_didRegisterInputHandler = true;
		}

		State = MovementState.Active;
		MoveMode = MovementMode.Idle;
		_targetVelocity = Vector2.zero;
		_airborne = false;
	}

	private void OnDisable()
	{
		if (_didRegisterInputHandler && InputManager.Instance != null)
		{
			InputManager.Instance.OnMovement.RemoveListener(HandleMovementInput);
		}
	}

	private void Awake()
	{
		// randomize idle swim direction when spawned in
		_lastNonZeroSwimVelX = UnityEngine.Random.value > 0.5f ? 1f : -1f;
	}

	private void Update()
	{
		if (State == MovementState.Active)
		{
			SetOrientation();
		}
	}

	private void FixedUpdate()
	{
		if (State != MovementState.Fixed)
		{
			HandleGeneralPhysics();
		}
		if (State == MovementState.Active)
		{
			HandleMovementIntention();
			HandleMovementPhysics();
		}
	}

	/** Input Handlers **/

	private void HandleMovementInput(Vector2 dir)
	{
		if (_takesInput)
		{
			if (dir != Vector2.zero)
			{
				SetTargetVelocity(dir * _inputMagnitude);
			}
			else
			{
				SetIdle();
			}
		}
	}

	/** Core Logic **/

	private void SetOrientation()
	{
		Vector2 visualVel = Rb.linearVelocity;

		if (Mathf.Abs(visualVel.x) > 0.0001f)
		{
			_lastNonZeroSwimVelX = visualVel.x;
		}

		_visualTilt = Mathf.Lerp(_visualTilt, _targetVisualTilt, _visualTiltLerpRate * Time.deltaTime);

		if (visualVel.magnitude > 0.001f)
		{
			Vector2 rectifiedVisualVel = new(_lastNonZeroSwimVelX, visualVel.y);
			_targetRot =
				Quaternion.LookRotation(
					(Vector3)rectifiedVisualVel.normalized + (_visualTilt * Vector3.down),
					Vector2.up
				) * Quaternion.Euler(0f, -90f, 0f);
		}

		_tiltParent.transform.rotation = Quaternion.RotateTowards(
			_tiltParent.transform.rotation,
			_targetRot,
			_tiltSpeed * Time.deltaTime
		);
	}

	private void HandleGeneralPhysics()
	{
		bool newAirborne = transform.position.y > 0f;
		if (newAirborne && !_airborne)
		{
			// halve speed when leaving water
			Rb.linearVelocity *= _waterBreakSpeedMultiplier;
		}
		_airborne = newAirborne;
		Rb.gravityScale = _airborne ? 1f : 0f;
	}

	private void HandleMovementPhysics()
	{
		if (!_airborne)
		{
			float lerpRatio = Mathf.Exp(Time.fixedDeltaTime * -_acceleration);
			Rb.linearVelocity = Vector2.Lerp(Rb.linearVelocity, MoveVelocity, 1f - lerpRatio);
		}
	}

	private void HandleMovementIntention()
	{
		switch (MoveMode)
		{
			case MovementMode.Idle:
			{
				HandleIdle();
				break;
			}
			case MovementMode.Velocity:
			{
				HandleTargetVelocity();
				break;
			}
			case MovementMode.SwimTowardPosition:
			{
				HandleSwimTowardPosition();
				break;
			}
			case MovementMode.SwimTowardTransform:
			{
				HandleSwimTowardTransform();
				break;
			}
			case MovementMode.SwimAwayPosition:
			{
				HandleSwimAwayPosition();
				break;
			}
			case MovementMode.SwimAwayTransform:
			{
				HandleSwimAwayTransform();
				break;
			}
		}
	}

	/** Public AbstractFishMovementController Methods **/

	public override void Knockback(Vector2 amount)
	{
		throw new NotImplementedException();
	}

	public override Vector2 GetPointingDirection()
	{
		throw new NotImplementedException();
	}

	public override void Ragdoll()
	{
		State = MovementState.Ragdoll;
		// remove rotation constraint
		Rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
	}

	public override void UnRagdoll()
	{
		State = MovementState.Active;
		// restore rotation constraint and (for now) reset rotation by setting it directly
		Rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
		Rb.SetRotation(0f);
	}

	/** Public Visual Methods **/

	[Title("Visual Functions")]
	[FoldoutGroup("Debug")]
	[Button]
	public void SetVisualTilt(float tilt)
	{
		_targetVisualTilt = tilt;
	}

	/** Public Movement Methods (the fun part) **/

	[Title("Movement Primitives")]
	[FoldoutGroup("Debug")]
	[Button]
	public void SetSwimSpeed(float speed)
	{
		_swimSpeed = speed;
	}

	[FoldoutGroup("Debug")]
	[Button]
	public void SetSlowdownDistance(float dist)
	{
		_slowDownDistance = dist;
	}

	[FoldoutGroup("Debug")]
	[Button]
	public void SetIdle(float speedOverride = -1)
	{
		MoveMode = MovementMode.Idle;
		_idleSwimSpeed = speedOverride >= 0f ? speedOverride : _defaultIdleSwimSpeed;
	}

	private void HandleIdle()
	{
		// move forwards slowly, turning/adjusting to stay within bounds.
		Vector2 travelDir = new(Mathf.Sign(_lastNonZeroSwimVelX), 0f);

		if (transform.position.x > MovementBounds.xMax)
		{
			travelDir.x = -1f;
		}
		else if (transform.position.x < MovementBounds.xMin)
		{
			travelDir.x = 1f;
		}
		if (transform.position.y > MovementBounds.yMax)
		{
			travelDir.y = -1f;
		}
		else if (transform.position.y < MovementBounds.yMin)
		{
			travelDir.y = 1f;
		}

		MoveVelocity = _idleSwimSpeed * travelDir;
	}

	[FoldoutGroup("Debug")]
	[Button]
	public void SetTargetVelocity(Vector2 vel)
	{
		MoveMode = MovementMode.Velocity;
		_targetVelocity = vel;
	}

	private void HandleTargetVelocity()
	{
		MoveVelocity = _targetVelocity;
	}

	[FoldoutGroup("Debug")]
	[Button]
	public void SetSwimTowardPosition(Vector2 pos, float stoppingDistance = 1f)
	{
		MoveMode = MovementMode.SwimTowardPosition;
		_targetPosition = pos;
		_stoppingDistance = stoppingDistance;
	}

	private void HandleSwimTowardPosition()
	{
		MoveTowardsPos(_targetPosition);
	}

	[FoldoutGroup("Debug")]
	[Button]
	public void SetSwimTowardTransform(Transform transform, float stoppingDistance = 1f)
	{
		MoveMode = MovementMode.SwimTowardTransform;
		_targetTransform = transform;
		_stoppingDistance = stoppingDistance;
	}

	private void HandleSwimTowardTransform()
	{
		MoveTowardsPos(_targetTransform.position);
	}

	// dash

	// swim away from target (respecting bounds)
	[FoldoutGroup("Debug")]
	[Button]
	public void SetSwimAwayPosition(Vector2 pos, float stoppingDistance = 3f)
	{
		MoveMode = MovementMode.SwimAwayPosition;
		_targetPosition = pos;
		_stoppingDistance = stoppingDistance;
	}

	private void HandleSwimAwayPosition()
	{
		MoveAwayPos(_targetPosition);
	}

	[FoldoutGroup("Debug")]
	[Button]
	public void SetSwimAwayTransform(Transform transform, float stoppingDistance = 3f)
	{
		MoveMode = MovementMode.SwimAwayTransform;
		_targetTransform = transform;
		_stoppingDistance = stoppingDistance;
	}

	private void HandleSwimAwayTransform()
	{
		MoveAwayPos(_targetTransform.position);
	}

	// Thrash
	[FoldoutGroup("Debug")]
	[Button]
	public void SetStruggle(float magnitude, float duration)
	{
		throw new NotImplementedException();
	}

	/** Helpers **/

	private void MoveTowardsPos(Vector2 pos)
	{
		Vector2 offset = pos - (Vector2)transform.position;
		float distance = offset.magnitude;

		// return to idle if close enough
		if (distance < _stoppingDistance)
		{
			SetIdle();
			return;
		}

		// otherwise, set velocity directly towards target, and slow down as you approach.
		// can expand on this logic in the future (slowdown based on speed/accel, PID, etc)
		// but this simple method seems to give enough of what we want.
		float speed = _swimSpeed * Mathf.Clamp01(Mathf.InverseLerp(0f, _slowDownDistance, distance));
		if (_slowDownDistance == 0f)
		{
			speed = _swimSpeed;
		}

		MoveVelocity = offset.normalized * speed;
	}

	private void MoveAwayPos(Vector2 pos)
	{
		Vector2 offset = pos - (Vector2)transform.position;

		// return to idle if close enough
		if (offset.magnitude > _stoppingDistance)
		{
			SetIdle();
			return;
		}

		Vector2 swimDir = -offset.normalized;

		// prevent jumping out of the water when swimming (for now)
		if (transform.position.y > -0.5f)
		{
			swimDir.y = Mathf.Min(swimDir.y, 0f);
			swimDir = swimDir.normalized;
		}

		MoveVelocity = swimDir * _swimSpeed;
	}

	/** Debug **/

#if UNITY_EDITOR

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		if (State != MovementState.Active)
		{
			return;
		}

		// movement direction
		Gizmos.color = Color.white;
		Gizmos.DrawRay(transform.position, MoveVelocity.normalized);

		// target position/transform (if applicable)
		switch (MoveMode)
		{
			case MovementMode.SwimTowardPosition:
			{
				Gizmos.color = Color.limeGreen;
				Gizmos.DrawWireSphere(_targetPosition, 0.2f);
				break;
			}
			case MovementMode.SwimAwayPosition:
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_targetPosition, 0.2f);
				break;
			}
			case MovementMode.SwimTowardTransform:
			{
				Gizmos.color = Color.limeGreen;
				Gizmos.DrawWireSphere(_targetTransform.position, 0.2f);
				break;
			}
			case MovementMode.SwimAwayTransform:
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_targetTransform.position, 0.2f);
				break;
			}
		}
	}
#endif

	/** Types **/

	public enum MovementMode
	{
		None, // this shouldn't happen to a fish. if a fish isn't doing anything it should be idle.
		Idle,
		Velocity,
		SwimTowardPosition,
		SwimTowardTransform,
		SwimAwayPosition,
		SwimAwayTransform,
	}
}
