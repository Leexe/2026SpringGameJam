using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "ShootBullet",
	story: "[_patternsController] Shoots [_count] Bullets From [_origin] to [_target]",
	category: "Action",
	id: "161bed99ab997a983defa1a9d8c0a47d"
)]
public partial class ShootBulletAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<int> Count = (BlackboardVariable<int>)3;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> Damage = (BlackboardVariable<float>)10;

	[SerializeReference]
	public BlackboardVariable<float> Spread = (BlackboardVariable<float>)45;

	[SerializeReference]
	public BlackboardVariable<float> BulletSpeed = (BlackboardVariable<float>)5;

	[SerializeReference]
	public BlackboardVariable<float> Lifetime = (BlackboardVariable<float>)5;

	[SerializeReference]
	public BlackboardVariable<float> AngleOffset = (BlackboardVariable<float>)0;

	[SerializeReference]
	public BlackboardVariable<bool> RandomSpread = (BlackboardVariable<bool>)false;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootBullet(
			Count,
			Damage,
			Spread,
			BulletSpeed,
			Lifetime,
			AngleOffset,
			Origin.Value,
			Target.Value,
			RandomSpread
		);
		return Status.Success;
	}
}
