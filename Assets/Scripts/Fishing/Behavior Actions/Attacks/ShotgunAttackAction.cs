using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "ShotgunAttack",
	story: "[_patternsController] Fires [_count] Shotgun Bullets From [_origin] to [_target]",
	category: "Action",
	id: "d1d99ed1d5e9f154b0c3d846819322f8"
)]
public partial class ShotgunAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<int> Count = (BlackboardVariable<int>)5;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> SpreadAngle = (BlackboardVariable<float>)45f;

	[SerializeReference]
	public BlackboardVariable<float> BulletSpeed = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> BulletDamage = (BlackboardVariable<float>)10f;

	[SerializeReference]
	public BlackboardVariable<float> BulletLifetime = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<float> AngleOffset = (BlackboardVariable<float>)0f;

	[SerializeReference]
	public BlackboardVariable<bool> RandomStartAngle = (BlackboardVariable<bool>)false;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootBullet(
			Count.Value,
			BulletDamage.Value,
			SpreadAngle.Value,
			BulletSpeed.Value,
			BulletLifetime.Value,
			AngleOffset.Value,
			Origin.Value,
			Target.Value,
			RandomStartAngle.Value
		);
		return Status.Success;
	}
}
