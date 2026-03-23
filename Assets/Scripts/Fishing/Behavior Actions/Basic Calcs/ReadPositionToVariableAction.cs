using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Read Position to Variable",
	story: "Read [target] position -> [variable]",
	category: "Action",
	id: "478a5762336cb039667bd03c3ac2457b"
)]
public partial class ReadPositionToVariableAction : Action
{
	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<Vector2> Variable;

	protected override Status OnStart()
	{
		if (Target.Value == null)
		{
			return Status.Failure;
		}

		Variable.Value = Target.Value.position;
		return Status.Success;
	}
}
