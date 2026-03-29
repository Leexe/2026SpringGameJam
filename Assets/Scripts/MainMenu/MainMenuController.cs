using Animancer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private AnimancerComponent _mainMenuAnimancer;

	[SerializeField]
	private AnimancerComponent _transitionAnimancer;

	[Header("Animation Clips")]
	[SerializeField]
	private AnimationClip _introSequenceClip;

	[SerializeField]
	private AnimationClip _fadeOutClip;

	private bool _canPressButtons;
	private bool _isSceneReady;
	private bool _isTransitionAnimationFinished;

	private void OnEnable()
	{
		LevelManager.Instance.OnSceneReady.AddListener(OnScenePreloaded);
	}

	private void OnDisable()
	{
		if (LevelManager.Instance)
		{
			LevelManager.Instance.OnSceneReady.RemoveListener(OnScenePreloaded);
		}
	}

	void Start()
	{
		PlayIntroSequence();
	}

	/* Button Functions */

	public void PlayButton()
	{
		if (_canPressButtons)
		{
			// State
			_canPressButtons = false;
			_isSceneReady = false;

			// Load Scene Async
			LevelManager.Instance.LoadSceneAsync(LevelManager.SceneNames.Game);

			// Play Transition
			PlayFadeOut();
		}
	}

	public void SettingsButton()
	{
		if (_canPressButtons) { }
	}

	/* Animation Functions */

	private void PlayFadeOut()
	{
		AnimancerState state = _transitionAnimancer.Play(_fadeOutClip);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FadeOut_Sfx);
		state.Events(this).OnEnd = () =>
		{
			state.Stop();
			_isTransitionAnimationFinished = true;
			TrySwitchScene();
		};
	}

	private void PlayIntroSequence()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FadeOut_Sfx);
		AnimancerState state = _transitionAnimancer.Play(_introSequenceClip);
		state.Events(this).OnEnd = () =>
		{
			state.Stop();
			_canPressButtons = true;
		};
	}

	/* Helper Functions */

	private void TrySwitchScene()
	{
		if (_isSceneReady && _isTransitionAnimationFinished)
		{
			LevelManager.Instance.ActivatePreloadedScene();
		}
	}

	private void OnScenePreloaded()
	{
		_isSceneReady = true;
		TrySwitchScene();
	}
}
