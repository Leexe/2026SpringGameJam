using UnityEngine;

// these hacks are done to get bullets working with the new minigame fish.
// doing so is not straightforward with refactors, because there is only
// 1 pattern controller in the scene (+ bulletpool), instead of on each
// fish, and the patterns controller requires a reference to the old minigame
// manager.
//
// this script:
// - listens for minigame start, and then passes patterncontroller to the active fish
// - handles patterncontroller's cleanup stuff

public class TempIntegrationHacks : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerFishingController _player;

	[SerializeField]
	private PatternsController _patternsController;

	private void OnEnable()
	{
		_player.OnMinigameStart += HandleMinigameStart;
		_player.OnMinigameEnd += HandleMinigameEnd;
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnMinigameStart -= HandleMinigameStart;
			_player.OnMinigameEnd -= HandleMinigameEnd;
		}
	}

	private void HandleMinigameStart(CursorController cursor, FishController fish, Rect rect)
	{
		//
		fish.BehaviorAgent.SetVariableValue("Patterns Controller (temp)", _patternsController);
	}

	private void HandleMinigameEnd(CursorController cursor, FishController fish, bool won)
	{
		//
		_patternsController.CleanupAll();
	}
}
