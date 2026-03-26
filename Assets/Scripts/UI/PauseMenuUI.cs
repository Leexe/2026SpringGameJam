using System;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	private void OnEnable()
	{
		GameManager.Instance.OnGameResume.AddListener(CloseUI);
		GameManager.Instance.OnGamePause.AddListener(OpenUI);
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGamePause.RemoveListener(CloseUI);
			GameManager.Instance.OnGameResume.RemoveListener(OpenUI);
		}
	}

	private void OpenUI()
	{
		_canvasGroup.alpha = 1;
		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = true;
	}

	private void CloseUI()
	{
		_canvasGroup.alpha = 0;
		_canvasGroup.blocksRaycasts = false;
		_canvasGroup.interactable = false;
	}
}
