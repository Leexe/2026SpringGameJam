using UnityEngine;
using UnityEngine.VFX;

public class CursorVFXController : MonoBehaviour
{
	[SerializeField]
	private VisualEffect _visualEffect;

	[SerializeField]
	private CursorController _cursor;

	private void FixedUpdate()
	{
		UpdateMovementParams();
	}

	private void UpdateMovementParams()
	{
		Vector2 velocity = _cursor.Movement.Rb.linearVelocity.normalized;
		_visualEffect.SetVector3("VelocityDir", new Vector3(velocity.x, velocity.y, 0));
	}

	public void StartSpeedLine()
	{
		_visualEffect.SendEvent("SpeedLinesStart");
	}

	public void StopSpeedLine()
	{
		_visualEffect.SendEvent("SpeedLinesStop");
	}

	public void StartSprintTrail()
	{
		_visualEffect.SendEvent("SprintTrailStart");
	}

	public void StopSprintTrail()
	{
		_visualEffect.SendEvent("SprintTrailStop");
	}
}
