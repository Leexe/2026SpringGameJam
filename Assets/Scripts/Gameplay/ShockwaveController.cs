using PrimeTween;
using UnityEngine;

public class ShockwaveController : MonoBehaviour
{
	[SerializeField]
	private float _endRadius = 5;

	[SerializeField]
	private float _duration = 1;

	[SerializeField]
	private SpriteRenderer _sprite;

	private Sequence _sequence;

	public void Awake()
	{
		transform.localScale = Vector3.zero;
		Color c = _sprite.color;
		c.a = 0f;
		_sprite.color = c;
	}

	public void Pulse()
	{
		_sequence = Sequence
			.Create()
			.Chain(Tween.Scale(transform, 0, _endRadius, _duration, Ease.OutCubic))
			.Group(Tween.Alpha(_sprite, 1, 0, _duration / 2, Ease.Linear));
	}

	private void OnDestroy()
	{
		if (_sequence.isAlive)
		{
			_sequence.Stop();
		}
	}
}
