using UnityEngine;

/// <summary>
/// An "interface" that dictates what methods ALL fish movement controllers should support.
/// How a fish moves and its movement methods are up to the specific implementation.
/// </summary>
public abstract class AbstractFishMovementController : MonoBehaviour
{
	[SerializeField]
	protected Rigidbody2D Rb;

	[SerializeField]
	protected Rect MovementBounds = new(-10f, -20f, 20f, 20f);

	public MovementState State { get; protected set; }
	public Rigidbody2D Rigidbody => Rb;
	public Rect Bounds => MovementBounds;

	public abstract Vector2 GetPointingDirection();
	public abstract void Knockback(Vector2 amount);

	public void SetBounds(Rect bounds)
	{
		MovementBounds = bounds;
	}

	public abstract void Ragdoll();
	public abstract void UnRagdoll();

	public enum MovementState
	{
		Active,
		Ragdoll,
		Fixed,
	}

#if UNITY_EDITOR

	protected virtual void OnDrawGizmos()
	{
		// bounds
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(MovementBounds.center, MovementBounds.size);
	}

#endif
}
