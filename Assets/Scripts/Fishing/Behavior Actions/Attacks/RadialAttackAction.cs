using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "RadialAttack",
	story: "[_patternsController] Fires [_count] Radial Bullets From [_origin] to [_target]",
	category: "Action",
	id: "b7adc63750859f6019aa377e53ffefc8"
)]
public partial class RadialAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<int> Count = (BlackboardVariable<int>)8;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> AngleOffset = (BlackboardVariable<float>)0f;

	[SerializeReference]
	public BlackboardVariable<float> BulletDamage = (BlackboardVariable<float>)10f;

	[SerializeReference]
	public BlackboardVariable<float> BulletSpeed = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<float> BulletLifetime = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<bool> RandomSpread = (BlackboardVariable<bool>)false;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootBullet(
			Count.Value + 1,
			BulletDamage.Value,
			360f, // spread - full circle for radial burst
			BulletSpeed.Value,
			BulletLifetime.Value,
			AngleOffset.Value,
			Origin.Value,
			Target.Value,
			RandomSpread.Value
		);

		if (Count.Value >= 3)
		{
			// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.BubbleRing_Sfx);
		}

		return Status.Success;
	}
}
