using Animancer;
using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVisuals : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerController _playerController;

	[SerializeField]
	private SpriteRenderer _hitBoxRenderer;

	[SerializeField]
	private VisualEffect _vfx;

	[SerializeField]
	private ShockwaveController _shockwave;

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

	[SerializeField]
	private AnimationClip _winAnim;

	[Header("Animation Settings")]
	[SerializeField]
	private float _circleTweenDuration = 0.5f;

	private float _lastRepairSecondsLeft = -100f;
	private bool _repairVFXPlaying = false;

	private Tween _hitBoxTween;
	private AnimancerState _animancerState;

	private void OnEnable()
	{
		InputManager.Instance.OnAnchorPerformed.AddListener(ShowCircle);
		InputManager.Instance.OnAnchorReleased.AddListener(HideCircle);

		_playerController.OnDie += PlayDeathAnimation;
		_playerController.OnFullyRepaired += PlayFullyRepairedVFX;

		GameManager.Instance.OnFadeInFinish.AddListener(PlayIdleAnimation);
		GameManager.Instance.OnGameWin.AddListener(PlayWinAnimation);

		_hitBoxRenderer.color *= new Color(1, 1, 1, 0);
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
			_playerController.OnFullyRepaired -= PlayFullyRepairedVFX;
		}

		if (GameManager.Instance)
		{
			GameManager.Instance.OnFadeInFinish.RemoveListener(PlayIdleAnimation);
			GameManager.Instance.OnGameWin.RemoveListener(PlayWinAnimation);
		}

		_hitBoxTween.Stop();
	}

	private void Update()
	{
		HandleMovementAnimations();
	}

	private void FixedUpdate()
	{
		HandleRepairVFXLogic();
	}

	private void HandleRepairVFXLogic()
	{
		// read repair values to determine if repairing
		// makes it so particles only show if you're actually making progress, and not any time you press space
		bool isRepairing = _lastRepairSecondsLeft > _playerController.RepairSecondsLeft;
		if (!_repairVFXPlaying && isRepairing)
		{
			StartRepairVFX();
			_repairVFXPlaying = true;
		}
		else if (_repairVFXPlaying && !isRepairing)
		{
			StopRepairVFX();
			_repairVFXPlaying = false;
		}

		_lastRepairSecondsLeft = _playerController.RepairSecondsLeft;
	}

	private void HandleMovementAnimations()
	{
		if (_playerController && _playerController.IsAlive && !_playerController.HasWon)
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

		if (isFromHit)
		{
			_vfx.SendEvent("OnDieToBullet");
		}
		else
		{
			_vfx.SendEvent("OnDieToInstability");
		}
	}

	private void PlayWinAnimation()
	{
		_animancer.Play(_winAnim);
		_hitBoxTween.Stop();
		_hitBoxRenderer.color *= new Color(1, 1, 1, 0);
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

	private void PlayIdleAnimation()
	{
		_animancer.Play(_idleNoThrustAnim);
	}

	private void StartRepairVFX()
	{
		_vfx.SendEvent("OnRepairStart");
	}

	private void StopRepairVFX()
	{
		_vfx.SendEvent("OnRepairStop");
	}

	private void PlayFullyRepairedVFX()
	{
		_vfx.SendEvent("OnFullyRepaired");
		_shockwave.Pulse();
	}
}
