using Animancer;
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

	private void Start()
	{
		PlayTransitionIn();
	}

	private void OnEnable()
	{
		GameManager.Instance.OnDeathAnimationFinish.AddListener(PlayTransitionOut);
	}

	private void OnDisable()
	{
		GameManager.Instance.OnDeathAnimationFinish.RemoveListener(PlayTransitionOut);
	}

	private void PlayTransitionIn()
	{
		_animancer.Play(_transitionIn);
	}

	private void PlayTransitionOut()
	{
		_animancer.Play(_transitionOut);
	}
}
