using System.Collections.Generic;
using Animancer;
using UnityEngine;

public class CoreProgressUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private AnimancerComponent _animancer;

	[Header("Animations")]
	[SerializeField]
	private List<AnimationClip> _coreProgressAnimations;

	[SerializeField]
	private AnimationClip _coreProgress0;

	private int _animationIndex;

	private void OnEnable()
	{
		GameManager.Instance.OnIncrementPhase.AddListener(IncrementAnimation);
		GameManager.Instance.OnFadeOutFinish.AddListener(ResetAnimation);
		ResetAnimation();
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnIncrementPhase.RemoveListener(IncrementAnimation);
			GameManager.Instance.OnFadeOutFinish.RemoveListener(ResetAnimation);
		}
	}

	private void IncrementAnimation(int _)
	{
		if (_animationIndex < _coreProgressAnimations.Count)
		{
			_animancer.Play(_coreProgressAnimations[_animationIndex++]);
		}
	}

	private void ResetAnimation()
	{
		_animationIndex = 0;
		if (_animationIndex < _coreProgressAnimations.Count)
		{
			_animancer.Play(_coreProgress0);
		}
	}
}
