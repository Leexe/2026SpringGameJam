using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Set Can Bite",
	story: "[Agent] sets CanBite to [value]",
	category: "Action",
	id: "75bdd372c85d6c462cb9ec791e445321"
)]
public partial class SetCanBiteAction : Action
{
	[SerializeReference]
	public BlackboardVariable<BiteController> Agent;

	[SerializeReference]
	public BlackboardVariable<bool> Value;

	protected override Status OnStart()
	{
		Agent.Value.SetReadyToBite(Value.Value);
		return Status.Success;
	}
}
