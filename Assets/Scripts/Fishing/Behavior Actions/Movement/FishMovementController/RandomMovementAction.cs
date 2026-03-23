using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "RandomMovement",
	story: "[Movement] with [Speed] in Random Direction",
	category: "Action",
	id: "2c13b589a9748922b3980c3575241683"
)]
public partial class RandomMovementAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Movement;

	[SerializeReference]
	public BlackboardVariable<float> Speed = (BlackboardVariable<float>)3f;

	[SerializeReference]
	public BlackboardVariable<float> Duration = (BlackboardVariable<float>)1f;

	[SerializeReference]
	public BlackboardVariable<bool> WaitForCompletion = (BlackboardVariable<bool>)false;

	private float _startTime;
	private Vector2 _swimVel;

	protected override Status OnStart()
	{
		if (Movement.Value == null)
		{
			Debug.LogError("RandomMovementAction: Movement is null!");
			return Status.Failure;
		}

		_swimVel = UnityEngine.Random.insideUnitCircle.normalized * Speed.Value;
		Movement.Value.SetSwimSpeed(Speed.Value);
		Movement.Value.SetTargetVelocity(_swimVel);

		if (!WaitForCompletion.Value)
		{
			return Status.Success;
		}

		_startTime = Time.time;
		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		StayInBounds();

		if (Time.time - _startTime >= Duration.Value)
		{
			return Status.Success;
		}

		return Status.Running;
	}

	protected override void OnEnd()
	{
		if (WaitForCompletion.Value && Movement.Value != null)
		{
			Movement.Value.SetIdle();
		}
	}

	/** Helpers **/

	private void StayInBounds()
	{
		Vector2 pos = Movement.Value.transform.position;
		Rect bounds = Movement.Value.Bounds;

		// keep fish in bounds
		if (pos.x < bounds.xMin)
		{
			_swimVel.x = Mathf.Abs(_swimVel.x);
		}
		if (pos.x > bounds.xMax)
		{
			_swimVel.x = -Mathf.Abs(_swimVel.x);
		}
		if (pos.y < bounds.yMin)
		{
			_swimVel.y = Mathf.Abs(_swimVel.y);
		}
		if (pos.y > bounds.yMax)
		{
			_swimVel.y = -Mathf.Abs(_swimVel.y);
		}
		Movement.Value.SetTargetVelocity(_swimVel);
	}
}
