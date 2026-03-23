using UnityEngine;

public class PlayerFishingSoundController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerFishingController _player;

	private void OnEnable()
	{
		_player.OnMinigameStart += PlayMinigameStartSFX;
		_player.OnMinigameEnd += PlayMinigameEndSFX;
		_player.OnHookCast += PlayHookCastSFX;
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnMinigameStart += PlayMinigameStartSFX;
			_player.OnMinigameEnd -= PlayMinigameEndSFX;
			_player.OnHookCast -= PlayHookCastSFX;
		}
	}

	private void PlayMinigameEndSFX(CursorController _, FishController __, bool didWin)
	{
		if (didWin)
		{
			// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Catch_Sfx);
		}
		else
		{
			// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LineBreak_Sfx);
		}
	}

	private void PlayMinigameStartSFX(CursorController _, FishController __, Rect ___)
	{
		// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Bite_Sfx);
	}

	private void PlayHookCastSFX()
	{
		// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Cast_Sfx);
	}
}
