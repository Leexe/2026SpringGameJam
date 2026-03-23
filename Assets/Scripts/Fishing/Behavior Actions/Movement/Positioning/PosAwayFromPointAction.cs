using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Pos Away From Point",
	story: "[Agent] finds pos away from [target] ( [min] - [max] ) -> [variable]",
	category: "Fish Movement/Positioning",
	id: "5e78dea387115b168e1bd3aeb62251fe"
)]
public partial class PosAwayFromPointAction : Action
{
	[SerializeReference]
	public BlackboardVariable<AbstractFishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<Vector2> Variable;

	[SerializeReference]
	public BlackboardVariable<Vector2> Target;

	[SerializeReference]
	public BlackboardVariable<float> Min;

	[SerializeReference]
	public BlackboardVariable<float> Max;

	protected override Status OnStart()
	{
		Rect bounds = Agent.Value.Bounds;
		Variable.Value = PositioningUtils.PosAwayFromPoint(bounds, Target.Value, Min.Value, Max.Value);

		return Status.Success;
	}
}
