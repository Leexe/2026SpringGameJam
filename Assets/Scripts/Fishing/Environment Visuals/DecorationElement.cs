using UnityEngine;

public class DecorationElement : MonoBehaviour
{
	[field: SerializeField]
	public SlotEnum Slot { get; private set; }

	/** Public Methods **/

	public virtual void Place(float depth)
	{
		return;
	}

	/** Types **/

	/// <summary>
	/// The "Location in the scene" that this backdrop corresponds to.
	/// </summary>
	public enum SlotEnum
	{
		Backdrop = 1,
		Sky = 2,
		SurfaceBack = 3,
		SurfaceFront = 4,
		SeafloorBack = 5,
		SeafloorFront = 6,
	}
}
