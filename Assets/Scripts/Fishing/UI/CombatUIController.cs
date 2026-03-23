using Sirenix.OdinInspector;
using UnityEngine;

// this entire class is temporary, until we settle on a visual design for combat UI.

public class CombatUIController : MonoBehaviour
{
	[FoldoutGroup("Debug")]
	[SerializeField]
	private bool _forceShow = false;

	[Header("References")]
	[SerializeField]
	private PlayerFishingController _player;

	[SerializeField]
	private RectTransform _bottomRoot;

	[SerializeField]
	private CombatHealthUI _fishHealthbar;

	[Header("Parameters")]
	[SerializeField]
	private float _bottomShownY = 0;

	[SerializeField]
	private float _bottomHiddenY = -75;

	[SerializeField]
	private float _bottomMoveSpeed = 25;

	private bool _shown;
	private float _bottomY;

	private void OnEnable()
	{
		if (_player == null)
		{
			return;
		}

		if (_bottomRoot != null)
		{
			_bottomY = _bottomHiddenY;
			Vector3 currentPos = _bottomRoot.anchoredPosition3D;
			currentPos.y = _bottomY;
			_bottomRoot.anchoredPosition3D = currentPos;
		}

		_player.OnMinigameStart += HandleMinigameStart;
		_player.OnMinigameEnd += HandleMinigameEnd;

		_fishHealthbar.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		if (_player != null)
		{
			_player.OnMinigameStart -= HandleMinigameStart;
		}
	}

	private void Update()
	{
		float goalPos = (_shown || _forceShow) ? _bottomShownY : _bottomHiddenY;
		_bottomY = Mathf.MoveTowards(_bottomY, goalPos, _bottomMoveSpeed * Time.deltaTime);

		if (_bottomRoot != null)
		{
			Vector3 currentPos = _bottomRoot.anchoredPosition3D;
			currentPos.y = _bottomY;
			_bottomRoot.anchoredPosition3D = currentPos;
		}
	}

	private void HandleMinigameStart(CursorController cursor, FishController fish, Rect rect)
	{
		_shown = true;

		_fishHealthbar.gameObject.SetActive(true);
		_fishHealthbar.AttachTo(fish.transform, fish.Health);
	}

	private void HandleMinigameEnd(CursorController _, FishController __, bool ___)
	{
		_shown = false;
		_fishHealthbar.Detach();
		_fishHealthbar.gameObject.SetActive(false);
	}
}
