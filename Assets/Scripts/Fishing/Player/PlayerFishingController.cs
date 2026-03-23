using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerFishingController : MonoBehaviour
{
	[FoldoutGroup("Debug")]
	[SerializeField]
	private bool _canCast = true;

	[FoldoutGroup("Debug")]
	[SerializeField]
	private bool _startCombatImmediately = false;

	[FoldoutGroup("Debug")]
	[SerializeField]
	private FishController _startCombatFish;

	[Header("References")]
	[SerializeField]
	private HookController _hookController;

	[SerializeField]
	private RodVisualController _rodVisualController;

	[Header("Minigame References (optional)")]
	[SerializeField]
	private CursorController _cursorController;

	[Header("Casting Parameters")]
	[SerializeField]
	private Vector2 _hookCastVelocity = new(0f, 0.1f);

	[SerializeField]
	private float[] _chargeDepths = new[] { 0f, 5f, 10f, 20f, 50f, 100f };

	[Header("Combat Parameters")]
	[Tooltip("Size of the bounds when entering combat.")]
	[SerializeField]
	private Vector2 _boundsSize = new(10f, 6f);

	//

	// tracks the fish that we are in combat with
	private FishController _combatFish = null;

	//

	public bool CanCast => _canCast;
	public HookController Hook => _hookController;
	public bool IsMinigameActive => _minigameActive;

	public event Action<FishingGameState, FishingGameState> OnStateChange; // state, prev

	public event Action OnHookCast;
	public event Action OnHookCastFail;
	public event Action<CursorController, FishController, Rect> OnMinigameStart;
	public event Action<CursorController, FishController, bool> OnMinigameEnd; // bool -> did win

	public event Action OnCastChargeBegin;
	public event Action<int, float> OnCastChargeStage; // stage
	public event Action OnCastCancel;

	private RodSO _equippedRod;
	private List<RodAttachmentSO> _equippedBobbers = new();
	private List<RodAttachmentSO> _equippedTrinkets = new();

	private bool _minigameActive;
	private bool _anchorHeld = false;

	private float _chargeTime = 0f;
	private int _chargeStage = 0;
	private float _plungeDepth = 0f;

	// factored in when computing bounds for minigame
	private float _floorDepth = 10000f;

	public FishingGameState State { get; private set; }

	private void Awake()
	{
		State = FishingGameState.Idle;

		if (_cursorController)
		{
			_cursorController.ReturnToHook(_hookController);
		}
	}

	private void OnEnable()
	{
		InputManager.Instance.OnAnchorPerformed.AddListener(OnAnchorAction);
		InputManager.Instance.OnAnchorReleased.AddListener(OnAnchorReleased);
		InputManager.Instance.OnAbilityPerformed.AddListener(OnAbilityAction);

		_hookController.Movement.OnEnterWater += OnHookEnterWater;
		_hookController.Movement.OnExitWater += OnHookExitWater;
		_hookController.Movement.OnReturnToIdle += OnHookReturnToIdle;
		_hookController.Biteable.OnBite += OnHookBite;
		_hookController.Movement.OnPlungeComplete += OnHookPlungeComplete;
		_rodVisualController.OnRodCastAnimComplete += OnRodCastAnimComplete;

		if (_cursorController)
		{
			_cursorController.OnLose += OnCombatLose;
		}
	}

	private void OnDisable()
	{
		if (InputManager.Instance)
		{
			InputManager.Instance.OnAnchorPerformed.RemoveListener(OnAnchorAction);
			InputManager.Instance.OnAnchorReleased.RemoveListener(OnAnchorReleased);
			InputManager.Instance.OnAbilityPerformed.RemoveListener(OnAbilityAction);
		}

		_hookController.Movement.OnEnterWater -= OnHookEnterWater;
		_hookController.Movement.OnExitWater -= OnHookExitWater;
		_hookController.Movement.OnReturnToIdle -= OnHookReturnToIdle;
		_hookController.Biteable.OnBite -= OnHookBite;
		_hookController.Movement.OnPlungeComplete -= OnHookPlungeComplete;
		_rodVisualController.OnRodCastAnimComplete -= OnRodCastAnimComplete;

		if (_cursorController)
		{
			_cursorController.OnLose -= OnCombatLose;
		}
	}

	private void Start()
	{
		if (_cursorController && _startCombatImmediately)
		{
			_cursorController.ReturnToHook(_hookController);
			TransitionState(FishingGameState.Casted); // force state to avoid warnings
			_hookController.transform.position = _startCombatFish.BiteController.BitePoint.position;
			OnHookBite(_startCombatFish);
		}
	}

	/** Event Handlers **/

	private void OnAnchorAction()
	{
		_anchorHeld = true;

		if (State == FishingGameState.Idle)
		{
			if (CanCast)
			{
				// if _combatFish isn't null, that means there's a caught fish dangling from the hook.
				if (_combatFish != null)
				{
					Destroy(_combatFish.gameObject);
					_combatFish = null;
					_rodVisualController.ResetAnimation();
				}
				else
				{
					// cast rod

					// state transition must happen first as the cast animation may instantly complete.
					TransitionState(FishingGameState.Casting);

					_rodVisualController.PlayCastAnimation();
				}
			}
			else
			{
				OnHookCastFail?.Invoke();
			}
		}
	}

	private void OnAnchorReleased()
	{
		_anchorHeld = false;
	}

	private void OnAbilityAction()
	{
		if (State == FishingGameState.Casted)
		{
			bool isPlunging = _hookController.Movement.State == HookMovementController.HookState.Plunging;

			if (_hookController.Movement.IsSubmerged && !isPlunging)
			{
				ReturnHook();
			}
		}
	}

	public void SetCastingEnabled(bool enabled)
	{
		_canCast = enabled;
	}

	public void ChangeRod(RodSO rod)
	{
		if (State != FishingGameState.Idle)
		{
			Debug.LogError("Attempted to change fishing rod when not possible to do so");
			return;
		}

		_equippedRod = rod;
		_hookController.Movement.SetMaxLineLength(rod.LineLength);
		_rodVisualController.SetRodVisual(rod);
	}

	public void ChangeBobber(IReadOnlyList<RodAttachmentSO> bobbers)
	{
		if (State != FishingGameState.Idle)
		{
			Debug.LogError("Attempted to change bobber when not possible to do so");
			return;
		}

		_equippedBobbers = (List<RodAttachmentSO>)bobbers;
		_rodVisualController.SetBobberVisual(_equippedBobbers);
		// skills are registered on combat start
	}

	public void ChangeTrinket(IReadOnlyList<RodAttachmentSO> trinkets)
	{
		if (State != FishingGameState.Idle)
		{
			Debug.LogError("Attempted to change trinket when not possible to do so");
			return;
		}

		_equippedTrinkets = (List<RodAttachmentSO>)trinkets;
		if (trinkets.Count > 0)
		{
			_rodVisualController.SetTrinketVisual(_equippedTrinkets);
		}
		// skills are registered on combat start
	}

	public void SetFloorDepth(float depth)
	{
		_floorDepth = depth;
	}

	private void TransitionState(FishingGameState state)
	{
		FishingGameState prevState = State;
		State = state;
		OnStateChange?.Invoke(state, prevState);
	}

	private void OnRodCastAnimComplete()
	{
		if (State == FishingGameState.Casting)
		{
			_chargeTime = 0f;
			_chargeStage = 0;
			_plungeDepth = 0f;

			bool hasChargeLevels =
				_chargeDepths.Length > 1 && (_equippedRod == null || _equippedRod.LineLength > _chargeDepths[1]);

			if (hasChargeLevels && _anchorHeld)
			{
				_hookController.Visuals.Absorb();

				TransitionState(FishingGameState.Charging);
				OnCastChargeBegin?.Invoke();
			}
			else
			{
				// immediately deploy hook
				DeployHook();
			}
		}
	}

	private void OnHookEnterWater()
	{
		if (State == FishingGameState.Casted)
		{
			if (_plungeDepth > 0f)
			{
				_hookController.Visuals.Flash(stage: 1); // not _chargeStage
				_hookController.Movement.PlungeToDepth(_plungeDepth);
				_plungeDepth = 0f;
			}
			else
			{
				// immediately call onHookPlungeComplete if no plunge (which enables noticeable + biteable)
				OnHookPlungeComplete();
			}
		}
	}

	private void OnHookPlungeComplete()
	{
		_hookController.Biteable.SetNoticeable(true);
		_hookController.Biteable.SetBiteable(true);
		_hookController.Visuals.StopFlash();
	}

	private void OnHookExitWater()
	{
		_hookController.Biteable.SetNoticeable(false);
		_hookController.Biteable.SetBiteable(false);

		if (State == FishingGameState.Casted)
		{
			_hookController.Movement.ReturnHook();
			TransitionState(FishingGameState.Returning);
		}

		// animation
		if (State is FishingGameState.Casted or FishingGameState.Returning)
		{
			// don't return to idle anim if there's a fish on the hook
			if (_combatFish == null)
			{
				_rodVisualController.ResetAnimation();
			}
		}
	}

	private void OnHookReturnToIdle()
	{
		if (State == FishingGameState.Returning)
		{
			TransitionState(FishingGameState.Idle);
		}
		else
		{
			Debug.LogError("TESTING - not implemented. need to implement something!!");
		}
	}

	private void OnHookBite(FishController fish)
	{
		if (State != FishingGameState.Casted)
		{
			Debug.LogError("Something bit hook when hook wasn't casted");
			return;
		}

		if (_cursorController)
		{
			// compute combat bounds. vertical bounds are centered around the hook, but may be further limited by:
			// 1. the water surface
			// 2. the fishing rod line length
			// 3. the sea floor
			Vector2 hookPos = _hookController.transform.position;

			float boundsTop = Mathf.Min(hookPos.y + (_boundsSize.y * 0.5f), -1f);
			float boundsBottom = Mathf.Max(hookPos.y - (_boundsSize.y * 0.5f), -_hookController.Movement.LineLength);
			float boundsLeft = hookPos.x - (0.5f * _boundsSize.x);

			// account for floor depth
			boundsBottom = Mathf.Max(boundsBottom, -_floorDepth + 0.5f);

			Rect bounds = new(boundsLeft, boundsBottom, _boundsSize.x, boundsTop - boundsBottom);

			// initialize combat
			_hookController.Movement.Grab(fish.BiteController.BitePoint);

			_combatFish = fish;
			_combatFish.OnFishLose += OnCombatWin;
			_combatFish.SignalCombatStart(_hookController.Biteable, _cursorController, bounds);

			_cursorController.BeginCombat(fish);

			// register skills
			_cursorController.Ability.RemoveAllAbilities();
			foreach (RodAttachmentSO bobber in _equippedBobbers)
			{
				if (bobber && bobber.AbilitySO != null)
				{
					_cursorController.Ability.AddAbility(bobber.AbilitySO.Id);
				}
			}
			foreach (RodAttachmentSO trinket in _equippedTrinkets)
			{
				if (trinket && trinket.AbilitySO != null)
				{
					_cursorController.Ability.AddAbility(trinket.AbilitySO.Id);
				}
			}
			_cursorController.Ability.ResetAbilities();

			_minigameActive = true;
			OnMinigameStart?.Invoke(_cursorController, _combatFish, bounds);
		}

		TransitionState(FishingGameState.Bitten);
	}

	private void OnCombatLose()
	{
		EndGame(false);
	}

	private void OnCombatWin()
	{
		EndGame(true);
	}

	private void EndGame(bool didWin)
	{
		_combatFish.OnFishLose -= OnCombatWin;

		if (_cursorController)
		{
			_combatFish.SignalCombatEnd(_hookController.Biteable, _cursorController, didWin);
			_cursorController.ReturnToHook(_hookController);
			_cursorController.Ability.RemoveAllAbilities();
			_minigameActive = false;
			OnMinigameEnd?.Invoke(_cursorController, _combatFish, didWin);
		}

		if (didWin)
		{
			// do not set _combatFish to null; it gets set to null on cast when fish is deleted
			_hookController.Grabbing.GrabFish(_combatFish);
		}
		else
		{
			_combatFish = null;
		}

		ReturnHook();
	}

	private void DeployHook()
	{
		// cast hook
		_hookController.Movement.CastHook(_hookCastVelocity.x, _hookCastVelocity.y);
		_hookController.Visuals.StopAbsorb();

		TransitionState(FishingGameState.Casted);
		OnHookCast?.Invoke();
	}

	private void ReturnHook()
	{
		_hookController.Biteable.SetNoticeable(false);
		_hookController.Biteable.SetBiteable(false);
		_hookController.Movement.ReturnHook();

		TransitionState(FishingGameState.Returning);
	}

	private void FixedUpdate()
	{
		switch (State)
		{
			case FishingGameState.Charging:
			{
				if (!_anchorHeld)
				{
					DeployHook();
				}
				else
				{
					// charge up?
					_chargeTime += Time.fixedDeltaTime;

					bool onLastStage =
						_chargeStage == _chargeDepths.Length - 1
						|| (_equippedRod != null && _chargeDepths[_chargeStage + 1] > _equippedRod.LineLength);

					// go up a stag every 1s
					if (!onLastStage && _chargeTime > 1f)
					{
						if (!onLastStage)
						{
							_chargeStage++;
							_hookController.Visuals.Pulse(stage: 1); // fixed 1, not _chargeStage.
							OnCastChargeStage.Invoke(_chargeStage, _chargeDepths[_chargeStage]);
						}

						_chargeTime = 0f;
					}
					if (onLastStage && _chargeTime > 3f)
					{
						_rodVisualController.ResetAnimation();
						_hookController.Visuals.StopFlash();
						_hookController.Visuals.StopAbsorb();
						TransitionState(FishingGameState.Idle);

						OnCastCancel?.Invoke();
					}

					_plungeDepth = _chargeDepths[_chargeStage];
				}

				break;
			}
		}
	}

	// types

	public enum FishingGameState
	{
		Idle, // hook hanging from end of rod
		Charging, // charging long cast
		Casting,
		Casted, // hook out, and able to move if underwater
		Bitten, // hook in mouth of some fish
		Returning, // hook returning to surface (with nothing)
	}
}
