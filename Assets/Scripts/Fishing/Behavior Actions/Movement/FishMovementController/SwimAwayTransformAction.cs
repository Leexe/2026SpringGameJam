using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Swim Away From Transform",
	story: "[Agent] swims away from [transform] until [distance] away or [duration] seconds",
	category: "Fish Movement/FishMovementController",
	id: "d020afef388d686d8cd1823ef70da183"
)]
public partial class SwimAwayTransformAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<Transform> Transform;

	[SerializeReference]
	public BlackboardVariable<float> Distance = new(0f);

	[SerializeReference]
	public BlackboardVariable<float> Duration = new(1000f);

	private float _timer;

	protected override Status OnStart()
	{
		if (Transform.Value == null)
		{
			return Status.Failure;
		}

		_timer = Duration.Value;

		Agent.Value.SetSwimAwayTransform(Transform.Value, Distance.Value);

		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		_timer -= Time.deltaTime;
		if (_timer < 0f)
		{
			return Status.Success;
		}

		if (Agent.Value.MoveMode != FishMovementController.MovementMode.SwimAwayTransform)
		{
			return Status.Success;
		}

		return Status.Running;
	}
}
