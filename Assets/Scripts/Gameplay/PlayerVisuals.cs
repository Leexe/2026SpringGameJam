using Animancer;
using PrimeTween;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerController _playerController;

	[SerializeField]
	private SpriteRenderer _hitBoxRenderer;

	[SerializeField]
	private AnimancerComponent _animancer;

	[Header("Animations")]
	[SerializeField]
	private AnimationClip _idleThrustAnim;

	[SerializeField]
	private AnimationClip _leftThrustAnim;

	[SerializeField]
	private AnimationClip _rightThrustAnim;

	[SerializeField]
	private AnimationClip _idleNoThrustAnim;

	[SerializeField]
	private AnimationClip _leftNoThrustAnim;

	[SerializeField]
	private AnimationClip _rightNoThrustAnim;

	[SerializeField]
	private AnimationClip _coreDeathAnim;

	[SerializeField]
	private AnimationClip _hitDeathAnim;

	[Header("Animation Settings")]
	[SerializeField]
	private float _circleTweenDuration = 0.5f;

	private Tween _hitBoxTween;
	private AnimancerState _animancerState;

	private void OnEnable()
	{
		InputManager.Instance.OnAnchorPerformed.AddListener(ShowCircle);
		InputManager.Instance.OnAnchorReleased.AddListener(HideCircle);
		_hitBoxRenderer.color *= new Color(1, 1, 1, 0);

		if (_playerController != null)
		{
			_playerController.OnDie += PlayDeathAnimation;
		}
	}

	private void OnDisable()
	{
		if (InputManager.Instance)
		{
			InputManager.Instance.OnAnchorPerformed.RemoveListener(ShowCircle);
			InputManager.Instance.OnAnchorReleased.RemoveListener(HideCircle);
		}

		if (_playerController != null)
		{
			_playerController.OnDie -= PlayDeathAnimation;
		}

		_hitBoxTween.Stop();
	}

	private void Update()
	{
		HandleMovementAnimations();
	}

	private void HandleMovementAnimations()
	{
		if (_playerController != null && _playerController.IsAlive)
		{
			Vector2 movementInput = _playerController.MovementInput;
			bool thrust = movementInput.y > 0f;

			AnimationClip animToPlay = thrust ? _idleThrustAnim : _idleNoThrustAnim;
			if (movementInput.x < 0f)
			{
				animToPlay = thrust ? _leftThrustAnim : _leftNoThrustAnim;
			}
			else if (movementInput.x > 0f)
			{
				animToPlay = thrust ? _rightThrustAnim : _rightNoThrustAnim;
			}

			if (_animancerState == null || _animancerState.Clip != animToPlay)
			{
				_animancerState = _animancer.Play(animToPlay);
			}
		}
	}

	private void PlayDeathAnimation(bool isFromHit)
	{
		AnimancerState deathState = isFromHit ? _animancer.Play(_hitDeathAnim) : _animancer.Play(_coreDeathAnim);

		deathState.Events(this).OnEnd ??= () =>
		{
			_hitBoxTween.Stop();
			_hitBoxRenderer.color *= new Color(1, 1, 1, 0);
			deathState.IsPlaying = false;
			if (_playerController != null)
			{
				_playerController.TriggerDeathAnimationFinished();
			}
		};
	}

	private void ShowCircle()
	{
		_hitBoxTween.Stop();
		if (_hitBoxRenderer)
		{
			_hitBoxTween = Tween.Alpha(_hitBoxRenderer, 1f, _circleTweenDuration);
		}
	}

	private void HideCircle()
	{
		_hitBoxTween.Stop();
		if (_hitBoxRenderer)
		{
			_hitBoxTween = Tween.Alpha(_hitBoxRenderer, 0f, _circleTweenDuration);
		}
	}
}
