using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Look for Biteable",
	story: "[Agent] scans for Biteable -> [biteable]",
	category: "Fish Detection",
	id: "bc56f649bdfbb5f589c85b10e894b6b0"
)]
public partial class LookForBiteableAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishController> Agent;

	[SerializeReference]
	public BlackboardVariable<Biteable> Biteable;

	protected override Status OnStart()
	{
		Biteable res = Agent.Value.TryNoticeBiteable();
		if (res)
		{
			Biteable.Value = res;
		}
		return res == null ? Status.Failure : Status.Success;
	}
}
