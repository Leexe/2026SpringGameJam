using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "WallAttack",
	story: "[_patternsController] Shoots [_count] Bullets in a Wall Pattern from [_origin] to [_target]",
	category: "Action",
	id: "9ab235fd5d877ae97607e591281b06e5"
)]
public partial class WallAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<int> Count = (BlackboardVariable<int>)7;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> SpaceInBetweenShots = (BlackboardVariable<float>)1f;

	[SerializeReference]
	public BlackboardVariable<float> WallAngle = (BlackboardVariable<float>)90f;

	[SerializeReference]
	public BlackboardVariable<float> AngleOffset = (BlackboardVariable<float>)0f;

	[SerializeReference]
	public BlackboardVariable<float> BulletDamage = (BlackboardVariable<float>)10f;

	[SerializeReference]
	public BlackboardVariable<float> BulletSpeed = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> BulletLifetime = (BlackboardVariable<float>)5f;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootWall(
			Count.Value,
			BulletDamage.Value,
			SpaceInBetweenShots.Value,
			WallAngle.Value,
			AngleOffset.Value,
			BulletSpeed.Value,
			BulletLifetime.Value,
			Origin.Value,
			Target.Value
		);
		return Status.Success;
	}
}
