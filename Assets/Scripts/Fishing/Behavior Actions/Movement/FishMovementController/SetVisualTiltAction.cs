using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Set Visual Tilt",
	story: "[Agent] sets visual tilt to [value]",
	category: "Fish Movement/FishMovementController",
	id: "cf0c7f2fca9e656a2eef08592e90a787"
)]
public partial class SetVisualTiltAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	protected override Status OnStart()
	{
		Agent.Value.SetVisualTilt(Value.Value);
		return Status.Success;
	}
}
