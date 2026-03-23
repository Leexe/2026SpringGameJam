using System;
using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public class RodVisualController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private SpriteRenderer _rodSprite;

	[SerializeField]
	private Transform _animationRoot; // subject to change, depending on how cast animation is done

	[Header("Animation")]
	[SerializeField]
	private float _idleAngle = -10f;

	[SerializeField]
	private float _castedAngle = -40f;

	[SerializeField]
	private float _castDuration = 0.6f;

	public event Action OnRodCastAnimComplete;

	private Sequence _currentSequence;

	private void Start()
	{
		ResetAnimation();
	}

	private void OnDisable()
	{
		_currentSequence.Complete();
	}

	[Button]
	public void PlayCastAnimation()
	{
		if (_currentSequence.isAlive)
		{
			_currentSequence.Stop();
		}

		var targetRot = Quaternion.Euler(0f, 0f, _castedAngle);

		if (transform.localRotation == targetRot)
		{
			Debug.Log("alo1");
			OnRodCastAnimComplete?.Invoke();
			return;
		}

		_currentSequence = Sequence
			.Create()
			.Chain(Tween.LocalRotation(transform, targetRot, _castDuration, Ease.InQuad))
			.OnComplete(() => OnRodCastAnimComplete?.Invoke());
	}

	[Button]
	public void ResetAnimation(bool instant = false)
	{
		if (_currentSequence.isAlive)
		{
			_currentSequence.Stop();
		}

		if (instant)
		{
			transform.localRotation = Quaternion.Euler(0f, 0f, _idleAngle);
		}
		else
		{
			var targetRot = Quaternion.Euler(0f, 0f, _idleAngle);

			if (transform.localRotation == targetRot)
			{
				Debug.Log("alo2");
				OnRodCastAnimComplete?.Invoke();
				return;
			}

			_currentSequence = Sequence
				.Create()
				.Chain(Tween.LocalRotation(transform, targetRot, _castDuration, Ease.InQuad))
				.OnComplete(() => OnRodCastAnimComplete?.Invoke());
		}
	}

	//

	public void SetRodVisual(RodSO rod)
	{
		_rodSprite.sprite = rod.RodSprite;
	}

	public void SetBobberVisual(List<RodAttachmentSO> bobber)
	{
		// Debug.LogWarning("Not implemented");
	}

	public void SetTrinketVisual(List<RodAttachmentSO> trinket)
	{
		// Debug.LogWarning("Not implemented");
	}
}
