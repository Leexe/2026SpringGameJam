using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "LaserAttack",
	story: "[PatternsController] Fires Laser From [Origin] toward [Target]",
	category: "Action",
	id: "8eca25cbe1078c65ca41595d3468dc84"
)]
public partial class LaserAttackAction : Action
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
	public BlackboardVariable<float> Length = (BlackboardVariable<float>)10f;

	[SerializeReference]
	public BlackboardVariable<float> Width = (BlackboardVariable<float>)0.5f;

	[SerializeReference]
	public BlackboardVariable<float> WarningTime = (BlackboardVariable<float>)1f;

	[SerializeReference]
	public BlackboardVariable<float> ActiveTime = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> DamagePerSecond = (BlackboardVariable<float>)20f;

	[SerializeReference]
	public BlackboardVariable<float> TurnRate = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<bool> TrackTarget = (BlackboardVariable<bool>)true;

	private bool _isComplete;
	private float _startTime;
	private float _duration;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootLaser(
			Length.Value,
			Width.Value,
			WarningTime.Value,
			ActiveTime.Value,
			DamagePerSecond.Value,
			TurnRate.Value,
			TrackTarget.Value,
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
