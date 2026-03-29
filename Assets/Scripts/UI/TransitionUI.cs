using Animancer;
using PrimeTween;
using UnityEngine;

public class TransitionUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private AnimancerComponent _animancer;

	[Header("Animation Clips")]
	[SerializeField]
	private AnimationClip _transitionIn;

	[SerializeField]
	private AnimationClip _transitionOut;

	[Header("Settings")]
	[SerializeField]
	private float _transitionOutDelay = 1f;

	private void Start()
	{
		PlayTransitionIn();
	}

	private void OnEnable()
	{
		GameManager.Instance.OnPlayerDeath.AddListener(PlayTransitionOut);
		GameManager.Instance.OnGameRestart.AddListener(PlayTransitionIn);
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnPlayerDeath.RemoveListener(PlayTransitionOut);
			GameManager.Instance.OnGameRestart.RemoveListener(PlayTransitionIn);
		}
	}

	private void PlayTransitionIn()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.CanPause = false;
		}
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FadeIn_Sfx);

		AnimancerState state = _animancer.Play(_transitionIn);
		state.Events(this).OnEnd = () =>
		{
			state.Stop();

			if (GameManager.Instance != null)
			{
				GameManager.Instance.OnFadeInFinish?.Invoke();
			}
		};
	}

	private void PlayTransitionOut()
	{
		Tween.Delay(
			_transitionOutDelay,
			() =>
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FadeOut_Sfx);

				AnimancerState fadeOutState = _animancer.Play(_transitionOut);
				fadeOutState.Events(this).OnEnd = () =>
				{
					fadeOutState.Stop();
					if (GameManager.Instance != null)
					{
						GameManager.Instance.OnFadeOutFinish?.Invoke();
						GameManager.Instance.RestartGame();
					}
				};
			}
		);
	}
}
