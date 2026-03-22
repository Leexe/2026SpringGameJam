using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : PersistentMonoSingleton<InputManager>
{
	// References
	public InputActionAsset InputActions;

	// Actions
	private InputAction _movementAction;
	private InputAction _interactAction;
	private InputAction _abilityAction;
	private InputAction _escapeAction;
	private InputAction _attackAction;
	private InputAction _continueStoryAction;

	// Events
	[HideInInspector]
	public UnityEvent<Vector2> OnMovement;

	[HideInInspector]
	public UnityEvent OnInteractPerformed;

	[HideInInspector]
	public UnityEvent OnAbilityPerformed;

	[HideInInspector]
	public UnityEvent OnAbilityReleased;

	[HideInInspector]
	public UnityEvent OnEscapePerformed;

	[HideInInspector]
	public UnityEvent OnAnchorPerformed;

	[HideInInspector]
	public UnityEvent OnAnchorReleased;

	[HideInInspector]
	public UnityEvent OnAnchorHeld;

	[HideInInspector]
	public UnityEvent OnContinueStoryPerformed;

	private const string PLAYER_ACTION_MAP = "Player";
	private const string UI_ACTION_MAP = "UI";

	/** Start Methods **/

	protected override void Awake()
	{
		base.Awake();
		EnablePlayerInput();
		EnableUIInput();
		SetupInputActions();
	}

	private void OnEnable()
	{
		EnablePlayerInput();
	}

	private void OnDisable()
	{
		DisablePlayerInput();
	}

	private void SetupInputActions()
	{
		_movementAction = InputSystem.actions.FindAction("Movement");
		_interactAction = InputSystem.actions.FindAction("Interact");
		_abilityAction = InputSystem.actions.FindAction("Ability1");
		_escapeAction = InputSystem.actions.FindAction("Escape");
		_attackAction = InputSystem.actions.FindAction("Attack");
		_continueStoryAction = InputSystem.actions.FindAction("ContinueStory");
	}

	/** Update Methods **/

	private void Update()
	{
		UpdateInputs();
	}

	private void UpdateInputs()
	{
		UpdateMovementVector(_movementAction, ref OnMovement);

		AddEventToAction(_interactAction, ref OnInteractPerformed);
		AddEventToAction(_abilityAction, ref OnAbilityPerformed);
		AddEventToAction(_escapeAction, ref OnEscapePerformed);
		AddEventToAction(_attackAction, ref OnAnchorPerformed);
		AddEventToActionRelease(_attackAction, ref OnAnchorReleased);
		AddEventToAction(_continueStoryAction, ref OnContinueStoryPerformed);

		AddEventToActionHold(_attackAction, ref OnAnchorHeld);

		AddEventToActionRelease(_abilityAction, ref OnAbilityReleased);
	}

	/// <summary>
	/// Updates a Vector3 variable depending on a movement input action
	/// </summary>
	/// <param name="inputAction">Input action was pressed</param>
	/// <param name="unityEvent">Unity Event To Trigger</param>
	private void UpdateMovementVector(InputAction inputAction, ref UnityEvent<Vector2> unityEvent)
	{
		Vector3 readVector = inputAction.ReadValue<Vector3>();
		unityEvent?.Invoke(new Vector2(readVector.x, readVector.z));
	}

	/// <summary>
	/// Checks every update if the input was pressed and calls the unity event
	/// </summary>
	/// <param name="inputAction">Input action was pressed</param>
	/// <param name="unityEvent">Unity Event To Trigger</param>
	private void AddEventToAction(InputAction inputAction, ref UnityEvent unityEvent)
	{
		if (inputAction.WasPressedThisFrame())
		{
			unityEvent?.Invoke();
		}
	}

	/// <summary>
	/// Checks every update if the input was held down and calls the unity event
	/// </summary>
	/// <param name="inputAction">Input action was pressed</param>
	/// <param name="unityEvent">Unity Event To Trigger</param>
	private void AddEventToActionHold(InputAction inputAction, ref UnityEvent unityEvent)
	{
		if (inputAction.IsPressed())
		{
			unityEvent?.Invoke();
		}
	}

	/// <summary>
	/// Checks every update if the input was released and calls the unity event
	/// </summary>
	/// <param name="inputAction">Input action was pressed</param>
	/// <param name="unityEvent">Unity Event To Trigger</param>
	private void AddEventToActionRelease(InputAction inputAction, ref UnityEvent unityEvent)
	{
		if (inputAction.WasReleasedThisFrame())
		{
			unityEvent?.Invoke();
		}
	}

	/// <summary>
	/// Enable Player Input
	/// </summary>
	public void EnablePlayerInput()
	{
		InputActions.FindActionMap(PLAYER_ACTION_MAP).Enable();
	}

	/// <summary>
	/// Disable Player Input
	/// </summary>
	public void DisablePlayerInput()
	{
		InputActions.FindActionMap(PLAYER_ACTION_MAP).Disable();
	}

	/// <summary>
	/// Enable UI Input
	/// </summary>
	public void EnableUIInput()
	{
		InputActions.FindActionMap(UI_ACTION_MAP).Enable();
	}

	/// <summary>
	/// Disable UI Input
	/// </summary>
	public void DisableUIInput()
	{
		InputActions.FindActionMap(UI_ACTION_MAP).Disable();
	}
}
