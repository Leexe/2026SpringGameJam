using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "SingleShotAttack",
	story: "[_patternsController] Fires a Single Bullet From [_origin] to [_target]",
	category: "Action",
	id: "85e84cd647b917ad6eeece6e28c60851"
)]
public partial class SingleShotAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> BulletDamage = (BlackboardVariable<float>)10f;

	[SerializeReference]
	public BlackboardVariable<float> BulletSpeed = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> BulletLifetime = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<float> AngleOffset = (BlackboardVariable<float>)0f;

	[SerializeReference]
	public BlackboardVariable<bool> RandomSpread = (BlackboardVariable<bool>)false;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootBullet(
			1, // count - single shot
			BulletDamage.Value,
			0f, // spread - no spread for single shot
			BulletSpeed.Value,
			BulletLifetime.Value,
			AngleOffset.Value,
			Origin.Value,
			Target.Value,
			RandomSpread.Value
		);
		return Status.Success;
	}
}
