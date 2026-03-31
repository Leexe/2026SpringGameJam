using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class StabilityMeterUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerController _player;

	[SerializeField]
	private Image _progressBar;

	[SerializeField]
	private Image _instabilityBar;

	[SerializeField]
	private RectTransform _marker;

	[SerializeField]
	private RectTransform _backgroundImage;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Settings")]
	[SerializeField]
	private float _tweenDuration = 0.1f;

	[SerializeField]
	private float _fadeDuration = 0.5f;

	[SerializeField]
	private Gradient _gradient;

	private float _backgroundHeight;
	private Tween _progressTween;
	private Tween _instabilityTween;
	private Tween _fadeTween;
	private bool _isHidden;

	private void Start()
	{
		_backgroundHeight = _backgroundImage.rect.height;
	}

	private void OnEnable()
	{
		if (_player != null)
		{
			_player.OnInstabilityProgressChanged += UpdateInstabilityUI;
			_player.OnRepairProgressChanged += UpdateProgressUI;
			_player.OnFullyRepaired += HandleFullyRepaired;
			_player.OnRepairFilledUp += HandleRepairFilledUp;
		}
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnInstabilityProgressChanged -= UpdateInstabilityUI;
			_player.OnRepairProgressChanged -= UpdateProgressUI;
			_player.OnFullyRepaired -= HandleFullyRepaired;
			_player.OnRepairFilledUp -= HandleRepairFilledUp;
		}
	}

	private void LateUpdate()
	{
		_marker.anchoredPosition = new(_marker.anchoredPosition.x, _backgroundHeight * _progressBar.fillAmount);
	}

	[Button]
	private void UpdateInstabilityUI(float seconds)
	{
		if (_isHidden)
		{
			return;
		}

		float percent = seconds / _player.SecondsToDie;
		_instabilityTween.Stop();
		_instabilityTween = Tween.UIFillAmount(_instabilityBar, percent, _tweenDuration);
	}

	[Button]
	private void UpdateProgressUI(float seconds)
	{
		if (_isHidden)
		{
			return;
		}

		float percent = seconds / _player.SecondsToDie;
		_progressTween.Stop();
		if (!Mathf.Approximately(_progressBar.fillAmount, percent))
		{
			_progressTween = Tween.UIFillAmount(_progressBar, percent, _tweenDuration);
		}
	}

	private void HandleFullyRepaired()
	{
		_isHidden = true;
		_fadeTween.Stop();
		_fadeTween = Tween.Alpha(_canvasGroup, 0f, _fadeDuration);
	}

	private void HandleRepairFilledUp()
	{
		_progressTween.Stop();
		_instabilityTween.Stop();
		_progressBar.fillAmount = _player.RepairSecondsLeft / _player.SecondsToDie;
		_instabilityBar.fillAmount = _player.DieSecondsLeft / _player.SecondsToDie;

		_isHidden = false;
		_fadeTween.Stop();
		_fadeTween = Tween.Alpha(_canvasGroup, 1f, _fadeDuration);
	}
}
