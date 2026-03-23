using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Set Circle Enabled",
	story: "[Circle] circle enabled = [value]",
	category: "Circle",
	id: "dfc9b7a2a0da35585dbaa751035514d4"
)]
public partial class SetCircleEnabledAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishCircleController> Circle;

	[SerializeReference]
	public BlackboardVariable<bool> Value = new(true);

	protected override Status OnStart()
	{
		Circle.Value.SetEnabled(Value.Value);
		return Status.Success;
	}
}
