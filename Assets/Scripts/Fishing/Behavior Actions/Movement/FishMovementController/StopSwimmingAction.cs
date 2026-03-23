using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Stop Swimming",
	story: "[Agent] stops swimming ( speed= [speedOverride] )",
	category: "Fish Movement/FishMovementController",
	id: "a4a7335e6358b949b6dd6c7de98c571e"
)]
public partial class StopSwimmingAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<float> SpeedOverride = new(-1f);

	protected override Status OnStart()
	{
		float speed = SpeedOverride == null ? -1 : SpeedOverride.Value;
		Agent.Value.SetIdle(speed);
		return Status.Success;
	}
}
