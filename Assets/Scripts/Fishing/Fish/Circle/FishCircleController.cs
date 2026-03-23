using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FishCircleController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private ZoningAttack _zoningAttack;

	[SerializeField]
	private Damageable _circleDamageable;

	[SerializeField]
	private CircleCollider2D _collider;

	[Header("Params")]
	[field: SerializeField]
	public bool Enabled { get; private set; } = true;

	[field: SerializeField]
	public float Size { get; private set; } = 2;

	[field: SerializeField]
	public float TargetSize { get; private set; } = 2;

	[field: SerializeField]
	public float SizeChangeSpeed { get; private set; } = 2;

	//

	public event Action OnSizeChangeStarted;
	public event Action OnSizeChangeEnded;

	//

	public float ChangeRate => Mathf.Sign(TargetSize - Size) * SizeChangeSpeed;

	/** Unity Messages **/

	private void Awake()
	{
		SetEnabled(false);
	}

	private void Update()
	{
		if (Size != TargetSize)
		{
			Size = Mathf.MoveTowards(Size, TargetSize, SizeChangeSpeed * Time.deltaTime);
			_collider.radius = 0.5f * Size;

			if (Size == TargetSize)
			{
				OnSizeChangeEnded?.Invoke();
				SizeChangeSpeed = 0f;
			}
		}
	}

	/** Public Methods **/

	[FoldoutGroup("Debug")]
	[Button]
	public void ChangeSize(float targetSize, float changeSpeed = -1f)
	{
		TargetSize = targetSize;
		SizeChangeSpeed = changeSpeed;

		if (changeSpeed <= 0f)
		{
			Size = targetSize;
			SizeChangeSpeed = 0f;
			_collider.radius = 0.5f * Size;
			return;
		}

		OnSizeChangeStarted?.Invoke();
	}

	public void SetEnabled(bool enabled)
	{
		Enabled = enabled;
		_zoningAttack.SetEnabled(enabled);
		_circleDamageable.SetDamageable(enabled);
	}
}
