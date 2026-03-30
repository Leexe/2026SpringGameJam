using PrimeTween;
using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float _tweenDuration = 1f;

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnWinScreenShow.AddListener(ShowWinAnimation);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnWinScreenShow.RemoveListener(ShowWinAnimation);
		}
	}

	private void ShowWinAnimation()
	{
		Tween.Alpha(_canvasGroup, 1f, _tweenDuration);
	}
}
