using UnityEngine;

public class HookController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private HookMovementController _movement;

	[SerializeField]
	private HookGrabbingController _grabbing;

	[SerializeField]
	private HookVisualsController _visuals;

	[SerializeField]
	private Biteable _biteable;

	public HookMovementController Movement => _movement;
	public HookGrabbingController Grabbing => _grabbing;
	public HookVisualsController Visuals => _visuals;
	public Biteable Biteable => _biteable;
}
