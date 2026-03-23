using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Set Slowdown Distance",
	story: "[Agent] sets slowdown distance to [value]",
	category: "Fish Movement/FishMovementController",
	id: "6d80bac0632349dcc021ca0c5acda230"
)]
public partial class SetSlowdownDistanceAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<float> Value = new(1f);

	protected override Status OnStart()
	{
		Agent.Value.SetSlowdownDistance(Value.Value);
		return Status.Success;
	}
}
