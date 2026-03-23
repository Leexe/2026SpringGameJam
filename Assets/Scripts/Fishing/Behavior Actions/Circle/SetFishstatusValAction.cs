using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "set fishstatus val",
	story: "Set [var] to [value]",
	category: "Action",
	id: "44f5f27d400033f05d0add9767fdfbc4"
)]
public partial class SetFishstatusValAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishStatus> Var;

	[SerializeReference]
	public BlackboardVariable<FishStatus> Value;

	protected override Status OnStart()
	{
		Var.Value = Value.Value;
		return Status.Success;
	}
}
