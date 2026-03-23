using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
	name: "Distance to Vector2",
	story: "Distance from [Target] to [position] [comparison] [value]",
	category: "Conditions",
	id: "36915340e1dd5947ab1d062066bd9e4f"
)]
public partial class DistanceToVector2Condition : Condition
{
	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<Vector2> Position;

	[Comparison(comparisonType: ComparisonType.All)]
	[SerializeReference]
	public BlackboardVariable<ConditionOperator> Comparison;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	public override bool IsTrue()
	{
		if (Target.Value == null)
		{
			return false;
		}

		float dist = ((Vector2)Target.Value.position - Position.Value).magnitude;

		return Comparison.Value switch
		{
			ConditionOperator.Equal => dist == Value.Value,
			ConditionOperator.NotEqual => dist != Value.Value,
			ConditionOperator.Greater => dist > Value.Value,
			ConditionOperator.Lower => dist < Value.Value,
			ConditionOperator.GreaterOrEqual => dist >= Value.Value,
			ConditionOperator.LowerOrEqual => dist <= Value.Value,
			_ => false,
		};
	}
}
