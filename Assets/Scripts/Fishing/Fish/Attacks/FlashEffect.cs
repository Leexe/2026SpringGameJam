using System;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
	private Transform _target;
	private Vector3 _offset;
	private Animator _animator;

	public event Action OnComplete;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void Initialize(Transform target, Vector3 offset, float flashTime)
	{
		_target = target;
		_offset = offset;

		if (_animator != null && flashTime > 0f)
		{
			AnimatorClipInfo[] clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
			if (clipInfo.Length > 0)
			{
				float clipLength = clipInfo[0].clip.length;
				_animator.speed = clipLength / flashTime;
			}
		}
	}

	private void Update()
	{
		if (_target != null)
		{
			transform.position = _target.position + _offset;
		}
	}

	public void OnFlashComplete()
	{
		OnComplete?.Invoke();
		Destroy(gameObject);
	}
}
