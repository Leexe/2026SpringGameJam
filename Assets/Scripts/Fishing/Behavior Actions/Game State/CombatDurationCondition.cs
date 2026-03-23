using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
	name: "Combat Duration",
	story: "[Agent] combat time [comparison] [value]",
	category: "Conditions",
	id: "fee17213117db78849a8a4dad00cf6d5"
)]
public partial class CombatDurationCondition : Condition
{
	[SerializeReference]
	public BlackboardVariable<FishController> Agent;

	[Comparison(comparisonType: ComparisonType.All)]
	[SerializeReference]
	public BlackboardVariable<ConditionOperator> Comparison;

	[SerializeReference]
	public BlackboardVariable<float> Value;

	public override bool IsTrue()
	{
		float combatTime = Time.time - Agent.Value.BattleStartTime;

		return Comparison.Value switch
		{
			ConditionOperator.Equal => combatTime == Value.Value,
			ConditionOperator.NotEqual => combatTime != Value.Value,
			ConditionOperator.Greater => combatTime > Value.Value,
			ConditionOperator.Lower => combatTime < Value.Value,
			ConditionOperator.GreaterOrEqual => combatTime >= Value.Value,
			ConditionOperator.LowerOrEqual => combatTime <= Value.Value,
			_ => false,
		};
	}
}
