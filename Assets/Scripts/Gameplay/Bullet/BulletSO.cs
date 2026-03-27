using UnityEngine;

[CreateAssetMenu(fileName = "BulletSO", menuName = "Gameplay/Bullet")]
public class BulletSO : ScriptableObject
{
	[Header("Rendering")]
	[SerializeField]
	private Mesh _mesh;

	[SerializeField]
	private Material _material;

	[Header("Hitbox")]
	[SerializeField]
	private float _hitboxRadius = 0.25f;

	[SerializeField]
	private int _damage = 1;

	[Header("Visual Interpolation")]
	[SerializeField]
	private float _visualScale = 1f;

	public Mesh Mesh => _mesh;
	public Material Material => _material;
	public float HitboxRadius => _hitboxRadius;
	public int Damage => _damage;
	public float VisualScale => _visualScale;
}
