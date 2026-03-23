using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Swim Towards Transform",
	story: "[Agent] swims towards [target] until [distance] away or [duration] seconds",
	category: "Fish Movement/FishMovementController",
	id: "d5d05f113cc326fdd64f4f22de6bfe2b"
)]
public partial class SwimTowardsTransformAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> Distance = new(0f);

	[SerializeReference]
	public BlackboardVariable<float> Duration = new(1000f);

	private float _timer;

	protected override Status OnStart()
	{
		if (Target.Value == null)
		{
			return Status.Failure;
		}

		_timer = Duration.Value;

		Agent.Value.SetSwimTowardTransform(Target.Value, Distance.Value);

		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		_timer -= Time.deltaTime;
		if (_timer < 0f)
		{
			return Status.Success;
		}

		if (Agent.Value.MoveMode != FishMovementController.MovementMode.SwimTowardTransform)
		{
			return Status.Success;
		}

		return Status.Running;
	}
}
