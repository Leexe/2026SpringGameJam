using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class StabilityMeterUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Image _progressBar;

	[SerializeField]
	private Image _instabilityBar;

	[SerializeField]
	private RectTransform _marker;

	[SerializeField]
	private RectTransform _backgroundImage;

	[Header("Tweens")]
	[SerializeField]
	private float _tweenDuration = 0.1f;

	private float _backgroundHeight;
	private Sequence _progressTween;
	private Tween _instabilityTween;

	private void Start()
	{
		_backgroundHeight = _backgroundImage.rect.height;
		UpdateInstabilityUI(0.3f);
		UpdateProgressUI(0);
	}

	[Button]
	private void UpdateInstabilityUI(float normalizedProgress)
	{
		_instabilityTween.Stop();
		_instabilityTween = Tween.UIFillAmount(_instabilityBar, normalizedProgress, _tweenDuration);
	}

	[Button]
	private void UpdateProgressUI(float normalizedProgress)
	{
		_progressTween.Stop();
		_progressTween = Sequence
			.Create()
			.Group(Tween.UIFillAmount(_progressBar, normalizedProgress, _tweenDuration))
			.Group(Tween.UIAnchoredPositionY(_marker, _backgroundHeight * normalizedProgress, _tweenDuration));
	}
}
