using PrimeTween;
using TMPro;
using UnityEngine;

public class PlayerFishingUIController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerFishingController _player;

	[SerializeField]
	private Transform _anchor;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private TMP_Text _depthText;

	[Header("Parameters")]
	[SerializeField]
	private Vector2 _offset = Vector2.one;

	/** State Vars **/

	private bool _shown = false;
	private Tween _opacityTween;

	/** Unity Messages **/

	private void OnEnable()
	{
		_player.OnCastChargeStage += HandleCastChargeStage;

		_player.OnHookCast += HandleCastChargeEnd;
		_player.OnCastCancel += HandleCastChargeEnd;

		_canvasGroup.alpha = 0f;
		_shown = false;
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnCastChargeStage -= HandleCastChargeStage;

			_player.OnHookCast -= HandleCastChargeEnd;
			_player.OnCastCancel -= HandleCastChargeEnd;
		}
	}

	private void LateUpdate()
	{
		transform.position = (Vector2)_anchor.transform.position + _offset;
	}

	/** Event Handlers **/

	private void HandleCastChargeStage(int stage, float depth)
	{
		if (!_shown)
		{
			_shown = true;
			if (_opacityTween.isAlive)
			{
				_opacityTween.Stop();
			}
			_opacityTween = Tween.Alpha(_canvasGroup, 1f, 0.5f);
		}

		_depthText.text = $"{depth}m";

		Tween.Scale(_depthText.transform, startValue: 1.1f, endValue: 1f, duration: 0.2f);
	}

	private void HandleCastChargeEnd()
	{
		if (_shown)
		{
			_shown = false;
			if (_opacityTween.isAlive)
			{
				_opacityTween.Stop();
			}
			_opacityTween = Tween.Alpha(_canvasGroup, 0f, 0.5f);
		}
	}
}
