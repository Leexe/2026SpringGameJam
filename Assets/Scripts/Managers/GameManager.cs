using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : PersistentMonoSingleton<GameManager>
{
	[Header("SO Dictionaries")]
	[SerializeField]
	private FishDictionarySO _fishDictionarySO;

	[SerializeField]
	private RodDictionarySO _rodDictionarySO;

	[SerializeField]
	private PlayerStatsSO _playerStatsSO;


	[SerializeField]
	private UICanvasController _uiCanvasController;

	private enum Views
	{
		MapView,
		FishingView,
		TownView,
	}

	[HideInInspector]
	public UnityEvent OnGamePause;

	[HideInInspector]
	public UnityEvent OnGameUnpause;

	public InventoryState Inventory { get; private set; } = new();
	public PlayerStatsState PlayerStats { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		DevInitializeNewGameState();

		PlayerStats = new(_playerStatsSO);
	}

	// we don't have a save file to load off of, but we need a "default starting inventory
	// state" (starter boat, starter rod, starting gold amt, etc.). We do that here.
	private void DevInitializeNewGameState()
	{
		Inventory.AddRod(_rodDictionarySO.GetRodSOById("StartingRod"));

		Inventory.AddRodAttachment(_rodDictionarySO.GetRodAttachmentSOById("AdvancedBobber"));

		Inventory.AddGold(1000);

		Inventory.EquipRod(Inventory.Rods[0]);
		Inventory.EquipRodBobber(null);
		Inventory.EquipRodTrinket(null);

		string[] fishIds = new string[] { };
		foreach (string id in fishIds)
		{
			Inventory.AddFish(FishInstance.GenerateFromSO(_fishDictionarySO.GetFishSOById(id)));
		}
	}

	public void AddFish(FishInstance fishToAdd)
	{
		Inventory.AddFish(fishToAdd);
	}

	public void SetTimeScale(float timeScale)
	{
		Time.timeScale = timeScale;

		if (Mathf.Approximately(timeScale, 1f))
		{
			OnGameUnpause?.Invoke();
		}
		else if (Mathf.Approximately(timeScale, 0f))
		{
			OnGamePause?.Invoke();
		}
	}

	public void FadeInUI()
	{
		if (_uiCanvasController != null)
		{
			_uiCanvasController.FadeInUI();
		}
	}

	public void FadeOutUI()
	{
		if (_uiCanvasController != null)
		{
			_uiCanvasController.FadeOutUI();
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
