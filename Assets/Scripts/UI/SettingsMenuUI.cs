using UnityEngine;

public class SettingsMenuUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	public void OpenSettings()
	{
		_canvasGroup.alpha = 1f;
		_canvasGroup.interactable = true;
		_canvasGroup.blocksRaycasts = true;
	}

	public void CloseSettings()
	{
		_canvasGroup.alpha = 0f;
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
	}
}
