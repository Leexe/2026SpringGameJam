using System;
using UnityEngine;

public class HookMovementController : MonoBehaviour
{
	[Header("Debug")]
	[SerializeField]
	private bool _readsInput = true;

	[SerializeField]
	private bool _startActive = false;

	[Header("References")]
	[SerializeField]
	private Rigidbody2D _rb;

	[SerializeField]
	private DistanceJoint2D _distanceJoint;

	[SerializeField]
	private Transform _rodEnd;

	[Header("Movement")]
	[Tooltip("How much the hook hangs from the end of the rod when idle")]
	[SerializeField]
	private float _idleHangDistance = 0.1f;

	[Tooltip("Linear drag experienced when hook is in air. This is physics system drag.")]
	[SerializeField]
	private float _airDrag = 0.4f;

	[Tooltip("Linear drag experienced when hook is in water. This is physics system drag.")]
	[SerializeField]
	private float _waterDrag = 4f;

	[Tooltip("Force applied to hook in direction of line when Up is pressed.")]
	[SerializeField]
	private float _pullForceScale = 4f;

	[Tooltip("Downward force applied to hook when Down is pressed.")]
	[SerializeField]
	private float _releaseForceScale = 4f;

	[Tooltip("Sideways force applied to hook when Left/Right are pressed.")]
	[SerializeField]
	private float _sideForceScale = 0f; // 4f

	[Tooltip("Returning force multiplier when hook moves to the side. Multiplies against _sideForceScale * distance.")]
	[SerializeField]
	private float _sideReturnForceMultiplier = 0.9f;

	[Header("Retraction")]
	[SerializeField]
	private float _baseRetractionSpeed = 4f;

	[SerializeField]
	private float _fastRetractionSpeed = 6.5f;

	[Tooltip("Time until fast retraction kicks in")]
	[SerializeField]
	private float _fastRetractionDelay = 3f;

	[Tooltip("Minimum depth at which fast retraction will actually kick in, once delay is up")]
	[SerializeField]
	private float _fastRetractionMinDepth = 8f;

	public event Action OnEnterWater;
	public event Action OnExitWater;
	public event Action<float, float> OnPlungeStart; // depth, speed
	public event Action OnPlungeComplete;
	public event Action OnReturnToIdle;
	public event Action OnLineLimitReached;

	public float LineLength { get; private set; } = 1000f; // what the maximum hook length should be

	public HookState State { get; private set; } = HookState.Idle;
	private Vector2 _movementVector = Vector2.zero;
	public bool IsSubmerged { get; private set; }

	private float _timeRetracting = 0f;
	private bool _isFastRetracting = false;
	private float _plungeDepth = 0f;
	private float _plungeSpeed = 10f;

	/* Unity Events */

	private void Awake()
	{
		if (_startActive)
		{
			CastHook();
		}

		IsSubmerged = false;

		if (!_distanceJoint.maxDistanceOnly)
		{
			Debug.LogError("Distance joint was not set to maxDistanceOnly. updating");
			_distanceJoint.maxDistanceOnly = true;
		}

		if (_startActive)
		{
			_distanceJoint.enabled = false;
		}
		else
		{
			_distanceJoint.enabled = true;
			_distanceJoint.distance = _idleHangDistance;
		}
	}

	private void OnEnable()
	{
		if (_readsInput)
		{
			InputManager.Instance.OnMovement.AddListener(HandleMovementInput);
		}
	}

	private void OnDisable()
	{
		if (_readsInput && InputManager.Instance)
		{
			InputManager.Instance.OnMovement.RemoveListener(HandleMovementInput);
		}
	}

	private void HandleMovementInput(Vector2 movement)
	{
		_movementVector = movement;
	}

	private void LateUpdate()
	{
		if (State == HookState.Idle)
		{
			// transform.position = HookIdlePosition;
		}
	}

	private void FixedUpdate()
	{
		// apply a force to prevent rb sleeping, since changing distanceJoint.distance doesn't wake it up (LOL).
		_rb.AddForce(Vector2.zero);

		/* handle submerged status and callbacks, regardless of state */

		bool isSubmerged = _rb.position.y < 0f;

		if (isSubmerged && !IsSubmerged)
		{
			OnEnterWater?.Invoke();
		}
		if (!isSubmerged && IsSubmerged)
		{
			OnExitWater?.Invoke();
		}
		IsSubmerged = isSubmerged;

		if (IsSubmerged)
		{
			_rb.linearDamping = _waterDrag;
			_rb.gravityScale = 0f;
		}
		else
		{
			_rb.linearDamping = _airDrag;
			_rb.gravityScale = 1f;
		}

		/* state handlers */

		switch (State)
		{
			case HookState.Idle:
			{
				break;
			}

			case HookState.Active:
			{
				// we need the if-statements, because repeatedly setting these breaks rb interpolation
				if (!_rb.simulated)
				{
					_rb.simulated = true;
				}
				if (_rb.bodyType != RigidbodyType2D.Dynamic)
				{
					_rb.bodyType = RigidbodyType2D.Dynamic;
				}

				if (IsSubmerged)
				{
					// horz movement
					_rb.AddForce(_sideForceScale * _movementVector.x * Vector2.right);

					// vertical movement
					if (_movementVector.y > 0f)
					{
						Vector2 direction = ((Vector2)_rodEnd.position - _rb.position).normalized;
						_rb.AddForce(_pullForceScale * direction);
					}
					else if (_movementVector.y < 0f)
					{
						// only if not at line limit
						if (_rb.position.y > -LineLength)
						{
							_rb.AddForce(_releaseForceScale * Vector2.down);
						}
					}

					// returning force
					float xOffset = _rb.position.x - _rodEnd.position.x;
					_rb.AddForce(-_sideReturnForceMultiplier * xOffset * Vector2.right);

					// clamp position and velocity if out of line
					// TODO: a less jarring implementation
					if (_rb.position.y < -LineLength)
					{
						_rb.position = new(_rb.position.x, -LineLength);
						_rb.linearVelocityY = Math.Max(_rb.linearVelocityY, 0f);

						OnLineLimitReached?.Invoke();
					}
				}

				break;
			}

			case HookState.Grabbed:
			{
				if (_rb.simulated)
				{
					_rb.simulated = false;
				}
				if (_rb.bodyType != RigidbodyType2D.Kinematic)
				{
					_rb.bodyType = RigidbodyType2D.Kinematic;
				}

				// enforce grabbed position (sus)
				transform.rotation = Quaternion.identity;
				transform.localPosition = new(0f, 0f, 0f);

				break;
			}
			case HookState.Retracting:
			{
				if (!_rb.simulated)
				{
					_rb.simulated = true;
				}
				if (_rb.bodyType != RigidbodyType2D.Dynamic)
				{
					_rb.bodyType = RigidbodyType2D.Dynamic;
				}

				_timeRetracting += Time.fixedDeltaTime;

				// fast retraction - double speed if this retracting thing is taking too long.
				// with distance check, so we don't fast retract in the last half second, which looks weird
				if (_timeRetracting > _fastRetractionDelay && _rb.position.y < -_fastRetractionMinDepth)
				{
					_isFastRetracting = true;
				}

				float retractionSpeed = _isFastRetracting ? _fastRetractionSpeed : _baseRetractionSpeed;

				_distanceJoint.distance = Mathf.MoveTowards(
					_distanceJoint.distance,
					_idleHangDistance,
					retractionSpeed * Time.fixedDeltaTime
				);

				if (GetDistanceToRodEnd() <= _idleHangDistance + 0.03f)
				{
					OnReturnToIdle?.Invoke();
					State = HookState.Idle;
				}

				break;
			}

			case HookState.Plunging:
			{
				if (!_rb.simulated)
				{
					_rb.simulated = true;
				}
				if (_rb.bodyType != RigidbodyType2D.Dynamic)
				{
					_rb.bodyType = RigidbodyType2D.Dynamic;
				}

				if (_rb.position.y < -_plungeDepth)
				{
					State = HookState.Active;
					OnPlungeComplete?.Invoke();
				}
				else
				{
					_rb.linearVelocityY = -_plungeSpeed;
				}

				break;
			}
		}
	}

	private float GetDistanceToRodEnd()
	{
		return (_rb.position + _distanceJoint.anchor - (Vector2)_rodEnd.position).magnitude;
	}

	public void InstantRetract(float length)
	{
		if (State != HookState.Retracting)
		{
			Debug.LogError("Called InstantRetract() when not in retracting state");
			return;
		}

		_distanceJoint.distance = Mathf.Min(_distanceJoint.distance, length);
	}

	public void PlungeToDepth(float depth, float speed = 18f)
	{
		if (State != HookState.Active)
		{
			Debug.LogError("Called PlungeToDepth() when not in active state");
			return;
		}

		_plungeDepth = depth;
		_plungeSpeed = speed;
		State = HookState.Plunging;

		OnPlungeStart?.Invoke(depth, speed);
	}

	public void SetMaxLineLength(float length)
	{
		if (State != HookState.Idle)
		{
			Debug.LogError("Called SetLineLength() when not in idle state");
			return;
		}

		LineLength = length;
	}

	public void CastHook(float vx = 0f, float vy = 0f)
	{
		if (State != HookState.Idle)
		{
			Debug.LogError("Called CastHook() when not in idle state");
			return;
		}

		_rb.linearVelocity = new Vector2(vx, vy);
		State = HookState.Active;
		_distanceJoint.enabled = false;
	}

	public void ForceReturnToIdle()
	{
		if (State == HookState.Idle)
		{
			Debug.LogError("Called ForceReturnToIdle() when already in idle state");
			return;
		}

		OnReturnToIdle?.Invoke();
		State = HookState.Idle;
	}

	// TEMP (REWRITE NEEDED)
	public void Grab(Transform anchor)
	{
		transform.SetParent(anchor);
		// note that local Z must also be set to 0 for rotating parents.
		transform.localPosition = new(0f, 0f, 0f);
		State = HookState.Grabbed;
	}

	public void UnGrab()
	{
		transform.SetParent(null);
	}

	public void ReturnHook()
	{
		if (State is not HookState.Grabbed and not HookState.Active and not HookState.Plunging)
		{
			Debug.LogError("Called ReleaseHook() when not in grabbed/active state");
			return;
		}

		if (State == HookState.Grabbed)
		{
			UnGrab();
		}

		if (State == HookState.Plunging)
		{
			OnPlungeComplete?.Invoke();
		}

		_distanceJoint.distance = GetDistanceToRodEnd() + 0.3f;
		_distanceJoint.enabled = true;
		_timeRetracting = 0f;
		_isFastRetracting = false;
		State = HookState.Retracting;
	}

	// types

	public enum HookState
	{
		Idle,
		Active,
		Grabbed,
		Retracting,
		Plunging,
	}
}
