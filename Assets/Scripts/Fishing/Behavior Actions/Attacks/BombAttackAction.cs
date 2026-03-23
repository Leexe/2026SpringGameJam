using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "BombAttack",
	story: "[PatternsController] Drops Bomb From [Origin] at [Target]",
	category: "Action",
	id: "c3d8bbda6b1f0a8c3b85996b858eb293"
)]
public partial class BombAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<bool> WaitForCompletion = (BlackboardVariable<bool>)false;

	[SerializeReference]
	public BlackboardVariable<float> WarningRadius = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> ExplosionRadius = (BlackboardVariable<float>)3f;

	[SerializeReference]
	public BlackboardVariable<float> WarningTime = (BlackboardVariable<float>)1.5f;

	[SerializeReference]
	public BlackboardVariable<float> ActiveTime = (BlackboardVariable<float>)0.5f;

	[SerializeReference]
	public BlackboardVariable<float> Damage = (BlackboardVariable<float>)50f;

	[SerializeReference]
	public BlackboardVariable<bool> SpawnAtTarget = (BlackboardVariable<bool>)true;

	[SerializeReference]
	public BlackboardVariable<Vector2> Offset = (BlackboardVariable<Vector2>)Vector2.zero;

	private bool _isComplete;
	private float _startTime;
	private float _duration;

	protected override Status OnStart()
	{
		if (PatternsController.Value == null)
		{
			Debug.LogError("BombAttackAction: PatternsController is null!");
			return Status.Failure;
		}

		if (Origin.Value == null)
		{
			Debug.LogError("BombAttackAction: Origin is null!");
			return Status.Failure;
		}

		PatternsController.Value.ShootBomb(
			WarningRadius.Value,
			ExplosionRadius.Value,
			WarningTime.Value,
			ActiveTime.Value,
			Damage.Value,
			SpawnAtTarget.Value,
			Offset.Value,
			Origin.Value,
			Target.Value
		);

		if (!WaitForCompletion.Value)
		{
			return Status.Success;
		}
		_duration = WarningTime.Value + ActiveTime.Value;
		_startTime = Time.time;
		_isComplete = false;
		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		if (Time.time - _startTime >= _duration)
		{
			_isComplete = true;
		}

		return _isComplete ? Status.Success : Status.Running;
	}
}
