using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Rigidbody2D _rb;

	[Header("Parameters")]
	[SerializeField]
	private float _baseMoveSpeed = 7f;
	[SerializeField]
	private float _repairSpeedMult = 0.5f;

	/** Public Fields **/

	public event Action OnDie;

	/** Private Fields **/

	private bool _isRepairInputHeld;
	private Vector2 _movementInput;
	private bool _isDying;

	/** Unity Messages **/

	private void OnEnable()
	{
		InputManager.Instance.OnMovement.AddListener(HandleMovement);
		InputManager.Instance.OnAnchorPerformed.AddListener(HandleAnchorPerformed);
		InputManager.Instance.OnAnchorReleased.AddListener(HandleAnchorReleased);

		_isRepairInputHeld = false;
		_isDying = false;
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
		HandleMovementPhysics();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		Die();
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

	/** Private Methods **/

	private void HandleMovementPhysics()
	{
		float speed = _baseMoveSpeed * (_isRepairInputHeld ? _repairSpeedMult : 1f);
		_rb.linearVelocity = speed * Vector2.ClampMagnitude(_movementInput, 1f);
	}

	private void Die()
	{
		if (_isDying)
		{
			return;
		}

		OnDie?.Invoke();
		Debug.Log("fuck you");
		// TBD
	}
}
