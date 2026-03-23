using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Swim Towards Position",
	story: "[Agent] swims towards position [position] until [distance] away or [duration] seconds",
	category: "Fish Movement/FishMovementController",
	id: "80a64e2c4d0dcf35b1148011082505ff"
)]
public partial class SwimTowardsPositionAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<Vector2> Position;

	[SerializeReference]
	public BlackboardVariable<float> Distance = new(0f);

	[SerializeReference]
	public BlackboardVariable<float> Duration = new(1000f);

	private float _timer;

	protected override Status OnStart()
	{
		if (Position.Value == null)
		{
			return Status.Failure;
		}

		_timer = Duration.Value;

		Agent.Value.SetSwimTowardPosition(Position.Value, Distance.Value);

		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		_timer -= Time.deltaTime;
		if (_timer < 0f)
		{
			return Status.Success;
		}

		if (Agent.Value.MoveMode != FishMovementController.MovementMode.SwimTowardPosition)
		{
			return Status.Success;
		}

		return Status.Running;
	}
}
