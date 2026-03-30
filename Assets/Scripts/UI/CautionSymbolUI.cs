using System;
using Animancer;
using UnityEngine;

public class CautionSymbolUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private AnimancerComponent _animancer;

	[Header("Animation Clips")]
	[SerializeField]
	private AnimationClip _invisibleClip;

	[SerializeField]
	private AnimationClip _flashingClip;

	private void OnEnable()
	{
		GameManager.Instance.Player.OnWarningStart += PlayFlashingClip;
		GameManager.Instance.Player.OnWarningEnd += PlayInvisibleClip;
	}

	private void OnDisable()
	{
		GameManager.Instance.Player.OnWarningStart -= PlayFlashingClip;
		GameManager.Instance.Player.OnWarningEnd -= PlayInvisibleClip;
	}

	private void PlayInvisibleClip()
	{
		_animancer.Play(_invisibleClip);
	}

	private void PlayFlashingClip()
	{
		_animancer.Play(_flashingClip);
	}
}
