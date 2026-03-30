using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class VignetteUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Image _vignetteImage;

	[Header("Tween Settings")]
	[SerializeField]
	private Color _warningColor;

	[SerializeField]
	private float _warningColorLower = 0.8f;

	[SerializeField]
	private float _warningPulseDuration = 0.5f;

	[SerializeField]
	private float _returnDuration = 0.5f;

	private Color _defaultColor;
	private Sequence _colorSequence;

	private void Start()
	{
		_defaultColor = _vignetteImage.color;
	}

	private void FlashWarning()
	{
		_colorSequence.Stop();
		_colorSequence = Sequence
			.Create(-1)
			.Chain(Tween.Color(_vignetteImage, _warningColor, _warningPulseDuration))
			.Chain(Tween.Color(_vignetteImage, _warningColor * _warningColorLower, _warningPulseDuration));
	}

	private void ReturnToDefault()
	{
		_colorSequence.Stop();
		_colorSequence = Sequence.Create().Chain(Tween.Color(_vignetteImage, _defaultColor, _returnDuration));
	}
}
