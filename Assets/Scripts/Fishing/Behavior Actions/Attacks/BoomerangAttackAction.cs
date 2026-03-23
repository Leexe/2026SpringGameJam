using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "BoomerangAttack",
	story: "[PatternsController] Throws [Count] Boomerangs From [Origin] toward [Target]",
	category: "Action",
	id: "17b59c0896d2146199e57ad16e8f561d"
)]
public partial class BoomerangAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<int> Count = (BlackboardVariable<int>)1;

	[SerializeReference]
	public BlackboardVariable<float> Damage = (BlackboardVariable<float>)10f;

	[SerializeReference]
	public BlackboardVariable<float> Speed = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<float> Curve = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> SpinSpeed = (BlackboardVariable<float>)360f;

	[SerializeReference]
	public BlackboardVariable<float> Lifetime = (BlackboardVariable<float>)3f;

	[SerializeReference]
	public BlackboardVariable<float> SpreadAngle = (BlackboardVariable<float>)30f;

	[SerializeReference]
	public BlackboardVariable<float> AngleOffset = (BlackboardVariable<float>)0f;

	[SerializeReference]
	public BlackboardVariable<float> ColliderRadius = (BlackboardVariable<float>)0.3f;

	[SerializeReference]
	public BlackboardVariable<Sprite> CustomSprite;

	protected override Status OnStart()
	{
		if (PatternsController.Value == null)
		{
			Debug.LogError("BoomerangAttackAction: PatternsController is null!");
			return Status.Failure;
		}

		if (Origin.Value == null)
		{
			Debug.LogError("BoomerangAttackAction: Origin is null!");
			return Status.Failure;
		}

		PatternsController.Value.ShootBoomerang(
			Count.Value,
			Damage.Value,
			Speed.Value,
			Curve.Value,
			SpinSpeed.Value,
			Lifetime.Value,
			SpreadAngle.Value,
			AngleOffset.Value,
			ColliderRadius.Value,
			Origin.Value,
			Target.Value,
			CustomSprite?.Value
		);

		return Status.Success;
	}
}
