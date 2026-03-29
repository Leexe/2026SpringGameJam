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
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnPlayerDeath.RemoveListener(PlayTransitionOut);
		}
	}

	private void PlayTransitionIn()
	{
		_animancer.Play(_transitionIn);
	}

	private void PlayTransitionOut()
	{
		Tween.Delay(
			_transitionOutDelay,
			() =>
			{
				AnimancerState fadeOutState = _animancer.Play(_transitionOut);
				fadeOutState.Events(this).OnEnd = GameManager.Instance.RestartGame;
			}
		);
	}
}
