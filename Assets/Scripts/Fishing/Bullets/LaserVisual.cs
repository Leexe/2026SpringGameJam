using UnityEngine;

public class LaserVisual : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private BoxCollider2D _collider;

	[SerializeField]
	private LaserDamage _laserDamage;

	[Header("Sprite Settings")]
	[Tooltip("Height of the ball portion in UNITS")]
	[SerializeField]
	private float _ballHeight = 0.5625f;

	[Tooltip("Empty space at bottom of sprite in UNITS")]
	[SerializeField]
	private float _emptyBottomSpace = 0.9375f;

	[Tooltip("Total height of original sprite in UNITS")]
	[SerializeField]
	private float _originalSpriteHeight = 3f;

	[Header("Position Offsets")]
	[Tooltip("Move the head sprite - negative moves toward fish")]
	[SerializeField]
	private float _headOffset = 0f;

	[Tooltip("Move the scaled beam sprite - positive moves further out")]
	[SerializeField]
	private float _beamExtraOffset = 0f;

	[SerializeField]
	private float _lengthMultiplier = 0.5f;

	[SerializeField]
	private bool _isVertical = true;

	private GameObject _headObject;
	private GameObject _beamObject;
	private SpriteRenderer _headRenderer;
	private SpriteRenderer _beamRenderer;

	private float _length;
	private float _originalBeamHeight;

	public void Initialize(float length, float width, float damagePerSecond)
	{
		_length = length;

		if (_laserDamage != null)
		{
			_laserDamage.damagePerSecond = damagePerSecond;
		}

		// Calculate original beam height
		_originalBeamHeight = _originalSpriteHeight - _ballHeight - _emptyBottomSpace;

		// Hide the original sprite - we'll use our own renderers
		if (_spriteRenderer != null)
		{
			_spriteRenderer.enabled = false;
		}

		SetupHeadRenderer();
		SetupBeamRenderer();
		UpdateCollider(length, width);
	}

	private void SetupHeadRenderer()
	{
		if (_spriteRenderer == null)
		{
			return;
		}

		_headObject = new GameObject("LaserHead");
		_headObject.transform.SetParent(transform);
		_headObject.transform.localRotation = Quaternion.identity;
		_headObject.transform.localScale = Vector3.one;
		_headObject.transform.localPosition = new Vector3(0f, _headOffset, 0f);
		_headObject.layer = gameObject.layer;

		_headRenderer = _headObject.AddComponent<SpriteRenderer>();
		_headRenderer.sprite = _spriteRenderer.sprite;
		_headRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
		_headRenderer.sortingOrder = _spriteRenderer.sortingOrder; // In front
	}

	private void SetupBeamRenderer()
	{
		if (_spriteRenderer == null)
		{
			return;
		}

		_beamObject = new GameObject("LaserBeam");
		_beamObject.transform.SetParent(transform);
		_beamObject.transform.localRotation = Quaternion.identity;
		_beamObject.layer = gameObject.layer;

		_beamRenderer = _beamObject.AddComponent<SpriteRenderer>();
		_beamRenderer.sprite = _spriteRenderer.sprite;
		_beamRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
		_beamRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1; // Behind head

		// Calculate how much to scale the beam to reach target length
		float targetBeamLength = (_length * _lengthMultiplier) - _ballHeight;
		float scaleY = targetBeamLength / _originalBeamHeight;
		scaleY = Mathf.Max(0.01f, scaleY);

		// Position calculation to align ball
		float originalBallCenterY = _emptyBottomSpace + (_ballHeight / 2f);
		float scaledBallCenterY = scaleY * originalBallCenterY;
		float offsetY = originalBallCenterY - scaledBallCenterY;

		// Add head offset + extra beam offset
		offsetY += _headOffset + _beamExtraOffset;

		_beamObject.transform.localPosition = new Vector3(0f, offsetY, 0f);
		_beamObject.transform.localScale = new Vector3(1f, scaleY, 1f);
		gameObject.AddComponent<LaserSpriteSyncer>().Initialize(_spriteRenderer, _headRenderer, _beamRenderer);
	}

	private void UpdateCollider(float length, float width)
	{
		if (_collider == null)
		{
			return;
		}

		_collider.enabled = true;

		if (_isVertical)
		{
			_collider.size = new Vector2(width, length);
			_collider.offset = new Vector2(0f, _headOffset + _emptyBottomSpace + (length / 2f));
		}
		else
		{
			_collider.size = new Vector2(length, width);
			_collider.offset = new Vector2(length / 2f, 0f);
		}
	}

	public void SetDirection(Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

		if (_isVertical)
		{
			angle -= 90f;
		}

		transform.rotation = Quaternion.Euler(0f, 0f, angle);
	}

	public void SetWarningState(bool isWarning)
	{
		if (_collider != null)
		{
			_collider.enabled = !isWarning;
		}
	}

	public void PlayForm(float duration)
	{
		if (_animator != null)
		{
			_animator.SetTrigger("Form");
		}
	}

	public void PlayBreak(float duration)
	{
		if (_animator != null)
		{
			_animator.SetTrigger("Break");
		}

		if (_collider != null)
		{
			_collider.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (_headObject != null)
		{
			Destroy(_headObject);
		}
		if (_beamObject != null)
		{
			Destroy(_beamObject);
		}
	}
}

/// <summary>
/// Syncs sprite changes from animator to head and beam renderers
/// </summary>
public class LaserSpriteSyncer : MonoBehaviour
{
	private SpriteRenderer _source;
	private SpriteRenderer _headTarget;
	private SpriteRenderer _beamTarget;

	public void Initialize(SpriteRenderer source, SpriteRenderer head, SpriteRenderer beam)
	{
		_source = source;
		_headTarget = head;
		_beamTarget = beam;
	}

	private void LateUpdate()
	{
		if (_source == null)
		{
			return;
		}

		if (_headTarget != null)
		{
			_headTarget.sprite = _source.sprite;
		}
		if (_beamTarget != null)
		{
			_beamTarget.sprite = _source.sprite;
		}
	}
}
