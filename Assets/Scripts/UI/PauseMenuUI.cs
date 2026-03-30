using Animancer;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Transition")]
	[SerializeField]
	private TransitionUI _transitionUI;

	private bool _canPressButtons = true;
	private bool _isSceneReady;
	private bool _isTransitionAnimationFinished;
	private bool _isSwitchingScene;

	private void OnEnable()
	{
		GameManager.Instance.OnGameResume.AddListener(CloseUI);
		GameManager.Instance.OnGamePause.AddListener(OpenUI);

		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.OnSceneReady.AddListener(OnScenePreloaded);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGamePause.RemoveListener(CloseUI);
			GameManager.Instance.OnGameResume.RemoveListener(OpenUI);
		}

		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.OnSceneReady.RemoveListener(OnScenePreloaded);
		}
	}

	private void OpenUI()
	{
		_canvasGroup.alpha = 1;
		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = true;
		_canPressButtons = true;
		AudioManager.Instance.PauseAmbience();
		AudioManager.Instance.PauseMusic();
		GameManager.Instance.ShowCursor();
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Pause_Sfx);
	}

	private void CloseUI()
	{
		if (_isSwitchingScene)
		{
			return;
		}

		_canvasGroup.alpha = 0;
		_canvasGroup.blocksRaycasts = false;
		_canvasGroup.interactable = false;
		_canPressButtons = false;
		AudioManager.Instance.ResumeAmbience();
		AudioManager.Instance.ResumeMusic();
		GameManager.Instance.HideCursor();
	}

	public void MainMenuButton()
	{
		if (_canPressButtons)
		{
			_canPressButtons = false;
			_isSceneReady = false;
			_isTransitionAnimationFinished = false;
			_isSwitchingScene = true;

			GameManager.Instance.CanPause = false;

			LevelManager.Instance.LoadSceneAsync(LevelManager.SceneNames.MainMenu);

			PlayFadeOut();
		}
	}

	private void PlayFadeOut()
	{
		_transitionUI.PlayTransitionOut(() =>
		{
			_isTransitionAnimationFinished = true;
			TrySwitchScene();
		});
	}

	private void TrySwitchScene()
	{
		if (_isSceneReady && _isTransitionAnimationFinished)
		{
			Time.timeScale = 1f;
			LevelManager.Instance.ActivatePreloadedScene();
		}
	}

	private void OnScenePreloaded()
	{
		_isSceneReady = true;
		TrySwitchScene();
	}
}
