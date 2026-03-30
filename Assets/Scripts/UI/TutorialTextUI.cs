using PrimeTween;
using UnityEngine;

public class TutorialTextUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvas;

	[SerializeField]
	private float _fadeDuration = 0.5f;

	private Tween _fadeTween;
	private bool _shown;

	private void OnEnable()
	{
		_canvas.alpha = 0f;
		GameManager.Instance.OnFadeInFinish.AddListener(ShowTutorialText);
		GameManager.Instance.OnGameStart.AddListener(HideTutorialText);
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnFadeInFinish.RemoveListener(ShowTutorialText);
			GameManager.Instance.OnGameStart.RemoveListener(HideTutorialText);
		}
	}

	private void ShowTutorialText()
	{
		if (!_shown)
		{
			_fadeTween.Stop();
			_fadeTween = Tween.Alpha(_canvas, 1f, _fadeDuration, startDelay: 4f);
			_shown = true;
		}
	}

	private void HideTutorialText()
	{
		_fadeTween.Stop();
		_fadeTween = Tween.Alpha(_canvas, 0f, _fadeDuration);
	}
}
