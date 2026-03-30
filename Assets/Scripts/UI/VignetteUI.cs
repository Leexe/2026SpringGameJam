using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class VignetteUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Image _vignetteImage;

	[Header("Warning State")]
	[SerializeField]
	private Color _warningColor;

	[SerializeField]
	private float _warningColorLower = 0.8f;

	[SerializeField]
	private float _warningPulseDuration = 0.5f;

	[Header("Default State")]
	[SerializeField]
	private float _returnDuration = 0.5f;

	[Header("Death State")]
	[SerializeField]
	private Color _deathColor;

	[SerializeField]
	private float _deathDuration = 0.5f;

	private Color _defaultColor;
	private Sequence _colorSequence;
	private bool _isDead;

	private void Start()
	{
		_defaultColor = _vignetteImage.color;
	}

	private void OnEnable()
	{
		GameManager.Instance.Player.OnWarningStart += FlashWarning;
		GameManager.Instance.Player.OnWarningEnd += ReturnToDefault;
		GameManager.Instance.OnFadeOutFinish.AddListener(ResetState);
		GameManager.Instance.OnPlayerDeath.AddListener(Death);
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.Player.OnWarningStart -= FlashWarning;
			GameManager.Instance.Player.OnWarningEnd -= ReturnToDefault;
			GameManager.Instance.OnFadeOutFinish.RemoveListener(ResetState);
			GameManager.Instance.OnPlayerDeath.RemoveListener(Death);
		}
	}

	private void FlashWarning()
	{
		if (_isDead)
		{
			return;
		}

		_colorSequence.Stop();
		_colorSequence = Sequence
			.Create(-1, Sequence.SequenceCycleMode.Yoyo)
			.Chain(Tween.Color(_vignetteImage, _warningColor, _warningPulseDuration))
			.Chain(Tween.Color(_vignetteImage, _warningColor * _warningColorLower, _warningPulseDuration));
	}

	private void ReturnToDefault()
	{
		if (_isDead)
		{
			return;
		}

		_colorSequence.Stop();
		_colorSequence = Sequence.Create().Chain(Tween.Color(_vignetteImage, _defaultColor, _returnDuration));
	}

	private void ResetState()
	{
		_isDead = false;
		ReturnToDefault();
	}

	private void Death()
	{
		_colorSequence.Stop();
		_colorSequence = Sequence.Create().Chain(Tween.Color(_vignetteImage, _deathColor, _deathDuration));
		_isDead = true;
	}
}
