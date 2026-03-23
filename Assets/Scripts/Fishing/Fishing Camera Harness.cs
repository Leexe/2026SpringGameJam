using UnityEngine;

public class FishingCameraHarness : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerFishingController _player;

	[SerializeField]
	private Transform _idleTarget;

	[SerializeField]
	private Transform _customCameraTarget;

	// Tween Values
	private readonly float _fovZoomMagnitude = 0.9f;
	private readonly float _fovZoomInDuration = 1f;
	private readonly float _fovZoomOutDuration = 1f;

	private bool _inMinigame = false;
	private Vector2 _minigameCenter;
	private bool _isHookSubmerged = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void OnEnable()
	{
		_player.OnMinigameStart += HandleMinigameStart;
		_player.OnMinigameEnd += HandleMinigameEnd;
		_player.Hook.Movement.OnEnterWater += HandleHookEnterWater;
		_player.Hook.Movement.OnExitWater += HandleHookExitWater;
	}

	// Update is called once per frame
	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnMinigameStart -= HandleMinigameStart;
			_player.OnMinigameEnd -= HandleMinigameEnd;
			if (_player.Hook != null)
			{
				_player.Hook.Movement.OnEnterWater += HandleHookEnterWater;
				_player.Hook.Movement.OnExitWater += HandleHookExitWater;
			}
		}
	}

	private void HandleMinigameStart(CursorController _, FishController __, Rect bounds)
	{
		_inMinigame = true;

		// use bounds as x center, and just center y around hook position at time of bite.
		_minigameCenter = new(bounds.center.x, _player.Hook.transform.position.y);

		UpdateCameraTarget();
	}

	private void HandleMinigameEnd(CursorController _, FishController __, bool won)
	{
		_inMinigame = false;
		UpdateCameraTarget();
	}

	private void HandleHookEnterWater()
	{
		_isHookSubmerged = true;
		UpdateCameraTarget();
		CameraManager.Instance.FOVTween(_fovZoomMagnitude, _fovZoomInDuration);
	}

	private void HandleHookExitWater()
	{
		_isHookSubmerged = false;
		UpdateCameraTarget();
	}

	private void UpdateCameraTarget()
	{
		if (_inMinigame)
		{
			_customCameraTarget.transform.position = _minigameCenter;
			CameraManager.Instance.SetTrackingTarget(_customCameraTarget);
		}
		else
		{
			CameraManager.Instance.SetTrackingTarget(_isHookSubmerged ? _player.Hook.transform : _idleTarget);
			CameraManager.Instance.ResetSize(_fovZoomOutDuration);
		}
	}
}
