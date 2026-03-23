using System;
using UnityEngine;

public class MeleeVisual : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	private MeleeDamage _meleeDamage;

	// [Header("Shape Settings")]
	// [SerializeField]
	// private MeleeShape _shape = MeleeShape.Arc;

	public enum MeleeShape
	{
		Arc, // Sword slash arc
		Circle, // Stomp/AOE
		Rectangle, // Thrust/stab
		Cone, // Breath/fan
	}

	private float _width;
	private float _height;
	private float _arcAngle;

	public void Initialize(float width, float height, float damage, float arcAngle = 90f, Color? color = null)
	{
		// WIP
		throw new NotImplementedException();
	}
}
