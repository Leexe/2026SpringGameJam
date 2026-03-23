using Animancer;
using UnityEngine;

public class FishVisualController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform _rotateParent;

	[SerializeField]
	private FishMovementController _movement;

	[SerializeField]
	private AnimancerComponent _animancer;

	[SerializeField]
	private AnimationClip _swimAnimation;

	[Header("Parameters")]
	[SerializeField, Tooltip("Movement speed at which the swim animation will play at 1x speed")]
	private float _referenceSpeed = 3f;

	[SerializeField, Tooltip("Minimum animation speed. Usually some small nonzero value")]
	private float _minAnimationSpeed = 0.6f;

	[SerializeField, Tooltip("Maximum animation speed")]
	private float _maxAnimationSpeed = 3f;

	/** Member Vars **/

	private AnimancerState _animancerState;

	/** Unity Messages **/

	private void Awake()
	{
		StartSwimAnimation();
	}

	private void Update()
	{
		if (_movement.State == AbstractFishMovementController.MovementState.Active)
		{
			UpdateSwimVisuals();
		}
		else if (_movement.State == AbstractFishMovementController.MovementState.Ragdoll)
		{
			StopSwimAnimation();
		}
	}

	/** Private Methods **/

	private void UpdateSwimVisuals()
	{
		if (_animancerState != null)
		{
			float animSpeed = _movement.MoveVelocity.magnitude / _referenceSpeed;
			_animancerState.Speed = Mathf.Clamp(animSpeed, _minAnimationSpeed, _maxAnimationSpeed);
		}
	}

	private void StartSwimAnimation()
	{
		if (_swimAnimation != null)
		{
			_animancerState = _animancer.Play(_swimAnimation);
		}
	}

	private void StopSwimAnimation()
	{
		if (_animancerState != null && _animancerState.IsPlaying)
		{
			_animancerState.IsPlaying = false;
			_animancerState.Time = 0f;
		}
	}

	/** Public Methods **/

	public AnimancerState PlayAnimation(AnimationClip clip)
	{
		// play animation clip on layer 1, which by default overrides lower layers
		return _animancer.Layers[1].Play(clip);
	}
}
