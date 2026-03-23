using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "ClusterShot",
	story: "[PatternsController] shoots cluster from [Origin] to [Target]",
	category: "Action",
	id: "b748717f73c86098f3572fd774e38fdf"
)]
public partial class ClusterShotAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<bool> WaitForCompletion = (BlackboardVariable<bool>)false;

	[Header("Initial Bullet")]
	[SerializeReference]
	public BlackboardVariable<float> InitialSpeed = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> InitialDamage = (BlackboardVariable<float>)10f;

	[Header("Explosion")]
	[Tooltip("Time before the initial bullet explodes")]
	[SerializeReference]
	public BlackboardVariable<float> FuseTime = (BlackboardVariable<float>)0.5f;

	[Header("Cluster Bullets")]
	[SerializeReference]
	public BlackboardVariable<int> ClusterCount = (BlackboardVariable<int>)5;

	[SerializeReference]
	public BlackboardVariable<float> ClusterSpeed = (BlackboardVariable<float>)3f;

	[SerializeReference]
	public BlackboardVariable<float> ClusterDamage = (BlackboardVariable<float>)5f;

	[SerializeReference]
	public BlackboardVariable<float> SpreadAngle = (BlackboardVariable<float>)60f;

	[SerializeReference]
	public BlackboardVariable<float> ClusterLifetime = (BlackboardVariable<float>)3f;

	[Header("Visuals")]
	[SerializeReference]
	public BlackboardVariable<float> InitialBulletSize = (BlackboardVariable<float>)0.3f;

	[Header("Direction")]
	[Tooltip("If true, cluster continues in initial direction. If false, cluster aims at target.")]
	[SerializeReference]
	public BlackboardVariable<bool> ClusterContinuesDirection = (BlackboardVariable<bool>)true;

	private float _startTime;
	private float _duration;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootCluster(
			InitialSpeed.Value,
			InitialDamage.Value,
			FuseTime.Value,
			ClusterCount.Value,
			ClusterSpeed.Value,
			ClusterDamage.Value,
			SpreadAngle.Value,
			ClusterLifetime.Value,
			InitialBulletSize.Value,
			ClusterContinuesDirection.Value,
			Origin.Value,
			Target.Value
		);

		if (!WaitForCompletion.Value)
		{
			return Status.Success;
		}

		_duration = FuseTime.Value + ClusterLifetime.Value;
		_startTime = Time.time;
		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		if (Time.time - _startTime >= _duration)
		{
			return Status.Success;
		}
		return Status.Running;
	}
}
