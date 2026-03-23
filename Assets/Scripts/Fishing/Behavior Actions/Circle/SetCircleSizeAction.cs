using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Set Circle Size",
	story: "[Circle] circle size = [size] with speed [speed]",
	category: "Circle",
	id: "fa36436206c82041ba3941a8bd40429f"
)]
public partial class SetCircleSizeAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishCircleController> Circle;

	[SerializeReference]
	public BlackboardVariable<float> Size = new(0f);

	[SerializeReference]
	public BlackboardVariable<float> Speed = new(-1);

	protected override Status OnStart()
	{
		Circle.Value.ChangeSize(Size.Value, Speed.Value);
		return Status.Success;
	}
}
