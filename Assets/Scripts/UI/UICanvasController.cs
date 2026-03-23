using PrimeTween;
using UnityEngine;

public class UICanvasController : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	// [SerializeField]
	// private InventoryController _inventoryController;

	[SerializeField]
	private float _fadeDuration;

	private Tween _alphaTween;

	public void FadeOutUI()
	{
		_alphaTween.Stop();
		_alphaTween = Tween
			.Alpha(_canvasGroup, 0f, _fadeDuration, Ease.InSine);
			// .OnComplete(() => _inventoryController.CloseInventory());
		_canvasGroup.blocksRaycasts = false;
	}

	public void FadeInUI()
	{
		_alphaTween.Stop();
		_alphaTween = Tween.Alpha(_canvasGroup, 1f, _fadeDuration, Ease.InSine);
		_canvasGroup.blocksRaycasts = true;
	}
}
