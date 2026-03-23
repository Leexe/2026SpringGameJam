using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Add Float",
	story: "[Target] Add [Value]",
	category: "Action",
	id: "b5a943fa49e3972cd1d2b1b7bb2d3bd5"
)]
public partial class AddFloatAction : Action
{
	[SerializeReference]
	public BlackboardVariable<float> Target;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	protected override Status OnStart()
	{
		Target.Value += Value;
		return Status.Success;
	}
}
