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

	[Header("Tweens")]
	[SerializeField]
	private float _tweenDuration = 0.1f;

	private float _backgroundHeight;
	private Tween _progressTween;
	private Tween _instabilityTween;

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
		}
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnInstabilityProgressChanged -= UpdateInstabilityUI;
			_player.OnRepairProgressChanged -= UpdateProgressUI;
		}
	}

	private void LateUpdate()
	{
		_marker.anchoredPosition = new(_marker.anchoredPosition.x, _backgroundHeight * _progressBar.fillAmount);
	}

	[Button]
	private void UpdateInstabilityUI(float seconds)
	{
		float percent = seconds / _player.SecondsToDie;
		_instabilityTween.Stop();
		_instabilityTween = Tween.UIFillAmount(_instabilityBar, percent, _tweenDuration);
	}

	[Button]
	private void UpdateProgressUI(float seconds)
	{
		float percent = seconds / _player.SecondsToDie;
		_progressTween.Stop();
		if (!Mathf.Approximately(_progressBar.fillAmount, percent))
		{
			_progressTween = Tween.UIFillAmount(_progressBar, percent, _tweenDuration);
		}
	}
}
