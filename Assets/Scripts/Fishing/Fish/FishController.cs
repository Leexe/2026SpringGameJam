using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

// todo: interfaceify
public class FishController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private AbstractFishMovementController _movement;

	[SerializeField]
	private BiteController _biteController;

	[SerializeField]
	private HealthController _health;

	[SerializeField]
	private BehaviorGraphAgent _behaviorGraphAgent;

	[SerializeField]
	private BiteSuccess _biteSuccessEventChannel;

	[SerializeField]
	private CursorLostCombat _cursorLostCombatEventChannel; // yikes

	[SerializeField]
	private FishLostCombat _fishLostCombatEventChannel; // yikes

	//

	public event System.Action OnFishLose;

	public BiteController BiteController => _biteController;
	public HealthController Health => _health;
	public AbstractFishMovementController Movement => _movement;

	// this is only used for integration hacks... delete soon.
	public BehaviorGraphAgent BehaviorAgent => _behaviorGraphAgent;

	public FishInstance FishInstance { get; private set; }
	public float BattleStartTime { get; private set; }

	private Rect _wanderBounds = new(-10f, -20f, 20f, 20f);

	//

	public void Init(FishInstance instance, Rect wanderBounds)
	{
		FishInstance = instance;
		// TODO: potentially other stuff
		_wanderBounds = wanderBounds;
		Movement.SetBounds(_wanderBounds);
	}

	private void OnEnable()
	{
		//
		if (_biteController != null)
		{
			_biteController.OnCanBite += OnCanBite;
		}

		_health.OnDeath.AddListener(OnDeath);
	}

	private void OnDisable()
	{
		//
		if (_biteController != null)
		{
			_biteController.OnCanBite -= OnCanBite;
		}

		_health.OnDeath.RemoveListener(OnDeath);
	}

	// ???
	public Biteable TryNoticeBiteable()
	{
		List<Biteable> biteables = _biteController.CheckBiteables();

		// TODO: filter by preference.

		if (biteables.Count > 0)
		{
			return biteables[0];
		}

		return null;
	}

	private void OnCanBite(Biteable biteable)
	{
		// just bite it
		if (biteable.ActiveFish == null)
		{
			biteable.Bite(this);
		}
	}

	private void OnDeath()
	{
		OnFishLose?.Invoke();
	}

	// called by rodcontroller
	public void SignalCombatStart(Biteable biteable, CursorController player, Rect bounds)
	{
		_health.Revive();
		_movement.SetBounds(bounds);
		_biteSuccessEventChannel.SendEventMessage(gameObject, biteable, player != null ? player.gameObject : null);

		BattleStartTime = Time.time;
	}

	public void SignalCombatEnd(Biteable biteable, CursorController player, bool playerWon)
	{
		if (playerWon)
		{
			_fishLostCombatEventChannel.SendEventMessage(gameObject, player.gameObject);
			// state transition handled by behavior graph
		}
		else
		{
			_cursorLostCombatEventChannel.SendEventMessage(player.gameObject, gameObject);
			// state transition handled by behavior graph
			Movement.SetBounds(_wanderBounds);
		}
	}

	//
}
