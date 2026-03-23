using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

public class ShockwaveController : MonoBehaviour
{
	[SerializeField]
	private GameObject _colliderGameObject;

	[SerializeField]
	private GameObject _parentGameObject;

	[SerializeField]
	private SpriteRenderer _sprite;

	[SerializeField]
	private VisualEffect _vfx;

	// Cached Fields from SO
	private LayerMask _fishLayer;
	private string _bulletTag;
	private Transform _playerTransform;

	// Cached Callbacks
	private Action _deleteCallback;
	private Action _startShockWaveCallback;
	private Action _startBuildUpCallback;

	// Primetween Sequence
	private Sequence _sequence;

	// Flags
	private bool _destroyBullets;
	private bool _followPlayer;

	private void Awake()
	{
		_deleteCallback = () => Delete();
		_startShockWaveCallback = StartShockWave;
		_startBuildUpCallback = StartBuildUp;
		_colliderGameObject.transform.localScale = Vector3.zero;

		// Attach relay to child collider object
		ShockwaveCollisionRelay relay = _colliderGameObject.AddComponent<ShockwaveCollisionRelay>();
		relay.Initialize(this);
	}

	public void Initialize(
		float startRadius,
		float endRadius,
		float startDelay,
		float duration,
		LayerMask fishLayer,
		string bulletTag,
		GameObject playerGameObject
	)
	{
		_fishLayer = fishLayer;
		_bulletTag = bulletTag;
		_playerTransform = playerGameObject.transform;

		_vfx.SetFloat("StartDelay", startDelay);
		_vfx.SetFloat("Duration", duration);

		_sequence = Sequence
			.Create()
			.ChainCallback(_startBuildUpCallback)
			.Chain(Tween.Scale(_colliderGameObject.transform, startRadius, 0, startDelay, Ease.Linear))
			.Group(Tween.Alpha(_sprite, 0.2f, 1, startDelay, Ease.Linear))
			.ChainCallback(_startShockWaveCallback)
			.Chain(Tween.Scale(_colliderGameObject.transform, 0, endRadius, duration, Ease.OutCubic))
			.Group(Tween.Alpha(_sprite, 1, 0, duration / 2, Ease.Linear, startDelay: duration / 2))
			.ChainCallback(_deleteCallback);
	}

	private void Delete(bool didWin = false)
	{
		_destroyBullets = false;
		_sequence.Stop();
		Destroy(_parentGameObject);
	}

	private void StartBuildUp()
	{
		_destroyBullets = false;
		_followPlayer = true;
		_vfx.SendEvent("OnBuildUp");
		// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ShockwaveWindUp_Sfx);
	}

	private void StartShockWave()
	{
		_destroyBullets = true;
		_followPlayer = false;
		_vfx.SendEvent("OnBurst");
		// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ShockwaveBoom_Sfx);
	}

	private void Update()
	{
		if (_followPlayer)
		{
			transform.position = _playerTransform.position;
		}
	}

	public void HandleCollision(Collider2D collision)
	{
		// Check for Layer Mask
		if (_destroyBullets)
		{
			if (collision.CompareTag(_bulletTag))
			{
				if (collision.TryGetComponent(out Bullet bullet))
				{
					bullet.Burst();
				}
			}
		}
	}
}

public class ShockwaveCollisionRelay : MonoBehaviour
{
	private ShockwaveController _controller;

	public void Initialize(ShockwaveController controller)
	{
		_controller = controller;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		_controller?.HandleCollision(collision);
	}
}
