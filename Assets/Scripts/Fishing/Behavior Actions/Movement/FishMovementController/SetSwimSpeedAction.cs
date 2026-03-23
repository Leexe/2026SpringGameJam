using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Set Swim Speed",
	story: "[Agent] sets swim speed to [value]",
	category: "Fish Movement/FishMovementController",
	id: "1fc2a400049e1adff2983ac8cd1d8f41"
)]
public partial class SetSwimSpeedAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	protected override Status OnStart()
	{
		Agent.Value.SetSwimSpeed(Value.Value);
		return Status.Success;
	}
}
