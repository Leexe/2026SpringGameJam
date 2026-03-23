using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
	[Header("References")]
	public GameObject Cinemachine;
	public BoxCollider2D BoundingBox;

	[Header("Default Values")]
	[SerializeField]
	private float _defaultFieldOfView = 4.5f;

	private CinemachineCamera _cinemachineCamera;
	private CinemachineConfiner2D _cinemachineConfiner2D;
	private CinemachineBasicMultiChannelPerlin _cinemachineMultiChannelPerlin;
	private Tween _fovTween;
	private Tween _cameraShakeAmplitudeTween;
	private Tween _cameraShakeFrequencyTween;

	protected override void Awake()
	{
		base.Awake();

		// Get References
		_cinemachineCamera = Cinemachine.GetComponent<CinemachineCamera>();
		_cinemachineConfiner2D = Cinemachine.GetComponent<CinemachineConfiner2D>();
		_cinemachineMultiChannelPerlin = Cinemachine.GetComponent<CinemachineBasicMultiChannelPerlin>();
	}

	public void SetTrackingTarget(Transform trackingTarget)
	{
		_cinemachineCamera.Target.TrackingTarget = trackingTarget;
	}

	public void RemoveTrackingTrack()
	{
		_cinemachineCamera.Target.TrackingTarget = null;
	}

	public void FOVTween(float mult, float duration)
	{
		_fovTween.Stop();
		_fovTween = Tween.Custom(
			this,
			_cinemachineCamera.Lens.OrthographicSize,
			_defaultFieldOfView * mult,
			duration,
			onValueChange: (target, newVal) => target._cinemachineCamera.Lens.OrthographicSize = newVal,
			Ease.InSine
		);
	}

	public void ResetSize(float duration)
	{
		_fovTween.Stop();
		_fovTween = Tween.Custom(
			this,
			_cinemachineCamera.Lens.OrthographicSize,
			_defaultFieldOfView,
			duration,
			onValueChange: (target, newVal) => target._cinemachineCamera.Lens.OrthographicSize = newVal,
			Ease.OutSine
		);
	}

	public void CameraShake(float amplitude, float frequency, float duration)
	{
		_cameraShakeAmplitudeTween.Complete();
		_cameraShakeFrequencyTween.Complete();
		_cameraShakeAmplitudeTween = Tween.Custom(
			amplitude,
			0f,
			duration,
			newVal => _cinemachineMultiChannelPerlin.AmplitudeGain = newVal
		);
		_cameraShakeFrequencyTween = Tween.Custom(
			frequency,
			0f,
			duration,
			newVal => _cinemachineMultiChannelPerlin.FrequencyGain = newVal
		);
	}

	public void SetBounds(Rect bounds)
	{
		BoundingBox.size = bounds.size;
		BoundingBox.offset = bounds.center - (Vector2)BoundingBox.transform.position;
		_cinemachineConfiner2D.InvalidateBoundingShapeCache();
	}
}
