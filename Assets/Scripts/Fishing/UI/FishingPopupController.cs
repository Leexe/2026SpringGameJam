using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class FishingPopupController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private RectTransform _popupTransform;

	[SerializeField]
	private TMP_Text _popupText;

	[Header("Parameters")]
	[SerializeField]
	private float _revealedOffset = 12f;

	[SerializeField]
	private float _hiddenOffset = -40f;

	[SerializeField]
	private float _transitionRate = 4f;

	[SerializeField]
	private float _lingerTime = 2f;

	private float _progress = 0f; // 0 to 1, 0 being fully hidden, and 1 fully out
	private float _lingerTimer = 0f;

	private void Update()
	{
		if (_lingerTimer > 0f)
		{
			_lingerTimer -= Time.deltaTime;
		}

		float targetProgress = _lingerTimer > 0f ? 1f : 0f;
		_progress = Mathf.MoveTowards(_progress, targetProgress, _transitionRate * Time.deltaTime);

		float t = EaseFunctions.EaseOutQuad(_progress);
		_popupTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(_hiddenOffset, _revealedOffset, t));
	}

	/// <summary>
	/// Makes the popup appear
	/// </summary>
	/// <param name="text">The message text. keep it to 2 lines</param>
	/// <param name="extraDuration">How many extra seconds you want the popup to appear for</param>
	/// <param name="immediate">Whether to have the popup appear immediately</param>
	[Button]
	public void Show(string text, float extraDuration = 0f, bool immediate = false)
	{
		_lingerTimer = _lingerTime + extraDuration;
		_popupText.text = text;

		if (immediate)
		{
			_progress = 1f;
		}
	}

	public void Hide(bool immediate = false)
	{
		_lingerTimer = 0f;

		if (immediate)
		{
			_progress = 0f;
		}
	}
}
