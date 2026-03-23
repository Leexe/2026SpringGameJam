using UnityEngine;

// used for dragging the fish up
public class HookGrabbingController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private HingeJoint2D _hinge;

	private FishController _grabbedFish;

	private void Awake()
	{
		_hinge.enabled = false;
	}

	public void GrabFish(FishController fish)
	{
		_grabbedFish = fish;

		fish.Movement.Ragdoll();
		Rigidbody2D fishRb = fish.Movement.Rigidbody;
		if (fishRb != null)
		{
			_hinge.connectedBody = fish.Movement.Rigidbody;
			_hinge.enabled = true;
		}
	}

	public void UnGrabFish()
	{
		if (_grabbedFish == null)
		{
			Debug.LogError("Called UnGrabFish when no fish grabbed");
			return;
		}

		_grabbedFish.Movement.UnRagdoll();
		_hinge.enabled = false;
		_hinge.connectedBody = null;

		_grabbedFish = null;
	}
}
