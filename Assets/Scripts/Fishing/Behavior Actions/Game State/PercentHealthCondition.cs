using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
	name: "Percent Health",
	story: "[Agent] health [comparison] [value]",
	category: "Conditions",
	id: "fc85f3df2ca3203ef4ddb3fe8ad351cb"
)]
public partial class PercentHealthCondition : Condition
{
	[SerializeReference]
	public BlackboardVariable<HealthController> Agent;

	[Comparison(comparisonType: ComparisonType.All)]
	[SerializeReference]
	public BlackboardVariable<ConditionOperator> Comparison;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	public override bool IsTrue()
	{
		float health = Agent.Value.GetNormalizedHealth;

		return Comparison.Value switch
		{
			ConditionOperator.Equal => health == Value.Value,
			ConditionOperator.NotEqual => health != Value.Value,
			ConditionOperator.Greater => health > Value.Value,
			ConditionOperator.Lower => health < Value.Value,
			ConditionOperator.GreaterOrEqual => health >= Value.Value,
			ConditionOperator.LowerOrEqual => health <= Value.Value,
			_ => false,
		};
	}
}
