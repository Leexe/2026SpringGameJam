using PrimeTween;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _hitBoxRenderer;

	[Header("Animation Settings")]
	[SerializeField]
	private float _circleTweenDuration = 0.5f;

	private Tween _hitBoxTween;

	private void OnEnable()
	{
		InputManager.Instance.OnAnchorPerformed.AddListener(ShowCircle);
		InputManager.Instance.OnAnchorReleased.AddListener(HideCircle);
		_hitBoxRenderer.color *= new Color(1, 1, 1, 0);
	}

	private void OnDisable()
	{
		if (InputManager.Instance)
		{
			InputManager.Instance.OnAnchorPerformed.AddListener(ShowCircle);
			InputManager.Instance.OnAnchorReleased.AddListener(HideCircle);
			_hitBoxTween.Stop();
		}
	}

	private void ShowCircle()
	{
		_hitBoxTween.Stop();
		_hitBoxTween = Tween.Alpha(_hitBoxRenderer, 1f, _circleTweenDuration);
	}

	private void HideCircle()
	{
		_hitBoxTween.Stop();
		_hitBoxTween = Tween.Alpha(_hitBoxRenderer, 0f, _circleTweenDuration);
	}
}
