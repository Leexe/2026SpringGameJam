using System;
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

	public void PlayTransitionIn(Action onEnd = null)
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FadeIn_Sfx);

		_animancer.UpdateMode = AnimatorUpdateMode.UnscaledTime;
		AnimancerState state = _animancer.Play(_transitionIn);
		state.Events(this).OnEnd = () =>
		{
			state.Stop();
			if (onEnd != null)
			{
				Tween.Delay(0f, onEnd, useUnscaledTime: true);
			}
		};
	}

	public void PlayTransitionOut(Action onEnd = null)
	{
		Tween.Delay(
			_transitionOutDelay,
			() =>
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.FadeOut_Sfx);

				_animancer.UpdateMode = AnimatorUpdateMode.UnscaledTime;
				AnimancerState fadeOutState = _animancer.Play(_transitionOut);
				fadeOutState.Events(this).OnEnd = () =>
				{
					fadeOutState.Stop();
					if (onEnd != null)
					{
						Tween.Delay(0f, onEnd, useUnscaledTime: true);
					}
				};
			},
			useUnscaledTime: true
		);
	}
}
