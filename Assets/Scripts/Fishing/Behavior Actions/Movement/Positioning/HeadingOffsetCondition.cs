using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
	name: "Heading Offset",
	story: "[Agent] heading offset to [target] is within [value]",
	category: "Fish Movement Conditions",
	id: "e14b81cb692374f3c67b8a7f45165c2a"
)]
public partial class HeadingOffsetCondition : Condition
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<GameObject> Target;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	public override bool IsTrue()
	{
		Vector2 dir = Agent.Value.GetPointingDirection();

		if (Target.Value == null || dir == Vector2.zero)
		{
			return false;
		}

		Vector2 offset = Target.Value.transform.position - Agent.Value.transform.position;
		float angle = Vector2.Angle(dir, offset);
		return angle <= Value.Value;
	}
}
