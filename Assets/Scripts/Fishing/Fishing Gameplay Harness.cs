using Stats;
using UnityEngine;

/// <summary>
/// Harnesses are the ONLY point of contact between gameobjects in the scene and
/// all other managers in the project (excluding InputManager).
/// FishingGameplayHarness primarily listens to events on PlayerFishingController
/// and updates GameManager's states accordingly.
/// </summary>
public class FishingGameplayHarness : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerFishingController _player;

	[SerializeField]
	private FishingPopupController _popup;

	[SerializeField]
	private CursorMovementController _cursorMovement;

	[SerializeField]
	private HealthController _healthController;

	[SerializeField]
	private AbilityController _abilityController;

	/** Unity Messages **/

	private void OnEnable()
	{
		if (GameManager.Instance == null)
		{
			Debug.LogError("FishingGameplayHarness references GameManager; it requires the Core scene to be loaded");
			return;
		}

		// Update stats if GameManager exists
		if (GameManager.Instance)
		{
			PlayerStatsState playerStats = GameManager.Instance.PlayerStats;
			// Cursor Movement Stats
			_cursorMovement.SetMoveSpeed(playerStats.GetFinalStat(StatType.Speed));
			_cursorMovement.SetReelSpeed(playerStats.GetFinalStat(StatType.ReelSpeed));

			// Health Stats
			_healthController.SetMaxHealth(playerStats.GetFinalStat(StatType.Health));
			_healthController.SetRegeneration(playerStats.GetFinalStat(StatType.RegenRate));
			_healthController.SetRegenDelay(playerStats.GetFinalStat(StatType.RegenDelay));
			_healthController.SetHurtIframeTime(playerStats.GetFinalStat(StatType.IFrameDuration));

			// Ability Stats
			_abilityController.SetStat(
				StatType.CooldownReduction,
				playerStats.GetFinalStat(StatType.CooldownReduction)
			);

			playerStats.OnStatChanged += UpdateStats;
		}

		GameManager.Instance.Inventory.OnRodLoadoutChanged += HandleRodLoadoutChanged;
		GameManager.Instance.Inventory.OnFishInventoryChanged += HandleFishInventoryChanged;
		_player.OnMinigameEnd += HandleMinigameEnd;
		_player.OnHookCastFail += HandleHookCastFail;
		_player.Hook.Movement.OnEnterWater += HandleHookEnterWater;
		_player.Hook.Movement.OnExitWater += HandleHookExitWater;
		_player.Hook.Movement.OnLineLimitReached += HandleLineLimitReached;
	}

	private void Start()
	{
		HandleRodLoadoutChanged();
		HandleFishInventoryChanged();
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.Inventory.OnRodLoadoutChanged -= HandleRodLoadoutChanged;
			GameManager.Instance.Inventory.OnFishInventoryChanged -= HandleFishInventoryChanged;

			GameManager.Instance.PlayerStats.OnStatChanged -= UpdateStats;

			// Reenable UI
			GameManager.Instance.FadeInUI();
		}

		if (_player != null)
		{
			_player.OnMinigameEnd -= HandleMinigameEnd;
			_player.OnHookCastFail -= HandleHookCastFail;
		}
	}

	/** Event Handlers - GameState **/

	private void HandleRodLoadoutChanged()
	{
		// TODO: merge into one function?
		_player.ChangeRod(GameManager.Instance.Inventory.EquippedRod);
		_player.ChangeBobber(GameManager.Instance.Inventory.EquippedRodBobbers);
		_player.ChangeTrinket(GameManager.Instance.Inventory.EquippedRodTrinkets);
	}

	private void HandleFishInventoryChanged()
	{
		// cannot cast while fish inventory full
		bool full = GameManager.Instance.Inventory.Fishes.Count == GameManager.Instance.Inventory.CurrentFishCapacity;
		_player.SetCastingEnabled(!full);
	}

	/** Event Handlers - Fishing Gameplay **/

	private void HandleMinigameEnd(CursorController _, FishController fish, bool won)
	{
		if (won)
		{
			FishInstance caughtFish = fish.FishInstance;

			if (caughtFish != null)
			{
				// store fish
				GameManager.Instance.AddFish(caughtFish);
			}
		}
	}

	private void HandleHookCastFail()
	{
		if (_popup != null)
		{
			_popup.Show("Inventory full.\nDiscard or sell a fish to cast again!");
		}
	}

	private void HandleLineLimitReached()
	{
		if (_popup != null)
		{
			_popup.Show("Out of fishing line!\nCan't go any deeper. (temp msg)");
		}
	}

	private void UpdateStats(StatChangedEventArgs e)
	{
		switch (e.StatType)
		{
			case StatType.Speed:
				_cursorMovement.SetMoveSpeed(e.FinalValue);
				break;
			case StatType.ReelSpeed:
				_cursorMovement.SetReelSpeed(e.FinalValue);
				break;
			case StatType.Health:
				_healthController.SetMaxHealth(e.FinalValue);
				break;
			case StatType.RegenRate:
				_healthController.SetRegeneration(e.FinalValue);
				break;
			case StatType.RegenDelay:
				_healthController.SetRegenDelay(e.FinalValue);
				break;
			case StatType.IFrameDuration:
				_healthController.SetHurtIframeTime(e.FinalValue);
				break;
			case StatType.CooldownReduction:
				_abilityController.SetStat(StatType.CooldownReduction, e.FinalValue);
				break;
		}
	}

	private void HandleHookEnterWater()
	{
		GameManager.Instance.FadeOutUI();
	}

	private void HandleHookExitWater()
	{
		if (!_player.IsMinigameActive)
		{
			GameManager.Instance.FadeInUI();
		}
	}
}
