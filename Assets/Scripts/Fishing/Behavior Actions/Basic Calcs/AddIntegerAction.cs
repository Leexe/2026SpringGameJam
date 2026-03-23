using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Add Integer",
	story: "[Target] Add [Value]",
	category: "Action",
	id: "cddd4804bf243199a6419daa982d37f8"
)]
public partial class AddIntegerAction : Action
{
	[SerializeReference]
	public BlackboardVariable<int> Target;

	[SerializeReference]
	public BlackboardVariable<int> Value;

	protected override Status OnStart()
	{
		Target.Value += Value;
		return Status.Success;
	}
}
