using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "MeleeAttack",
	story: "[_patternsController] spawns melee attack from [_origin] to [_target]",
	category: "Action",
	id: "c7708177b3054547b6254554a924ffbe"
)]
public partial class MeleeAttackAction : Action
{
	[SerializeReference]
	public BlackboardVariable<PatternsController> PatternsController;

	[SerializeReference]
	public BlackboardVariable<Transform> Origin;

	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<bool> WaitForCompletion = (BlackboardVariable<bool>)false;

	[Header("Shape")]
	[Tooltip("0=Arc, 1=Circle, 2=Rectangle, 3=Cone")]
	[SerializeReference]
	public BlackboardVariable<int> Shape = (BlackboardVariable<int>)0f;

	[SerializeReference]
	public BlackboardVariable<float> Width = (BlackboardVariable<float>)2f;

	[SerializeReference]
	public BlackboardVariable<float> Height = (BlackboardVariable<float>)1.5f;

	[SerializeReference]
	public BlackboardVariable<float> ArcAngle = (BlackboardVariable<float>)90f;

	[Header("Timing")]
	[SerializeReference]
	public BlackboardVariable<float> WarningTime = (BlackboardVariable<float>)0.5f;

	[SerializeReference]
	public BlackboardVariable<float> ActiveTime = (BlackboardVariable<float>)0.3f;

	[Header("Position")]
	[SerializeReference]
	public BlackboardVariable<float> SpawnDistance = (BlackboardVariable<float>)1f;

	[SerializeReference]
	public BlackboardVariable<Vector2> Offset = (BlackboardVariable<Vector2>)Vector2.zero;

	[Header("Damage")]
	[SerializeReference]
	public BlackboardVariable<float> Damage = (BlackboardVariable<float>)20f;

	[Tooltip("Only hit the target once per attack")]
	[SerializeReference]
	public BlackboardVariable<bool> HitOnce = new BlackboardVariable<bool>(true);

	[Header("Visuals")]
	[SerializeReference]
	public BlackboardVariable<float> Transparency = (BlackboardVariable<float>)1f;

	[Header("Tracking")]
	[Tooltip("How slow the warning tracks the player (0 = instant, higher = slower)")]
	[SerializeReference]
	public BlackboardVariable<float> TrackingLag = new BlackboardVariable<float>(0.5f);

	[Header("Knockback")]
	[SerializeReference]
	public BlackboardVariable<float> KnockbackForce = new BlackboardVariable<float>(5f);

	[Tooltip("True: knockback away from attack center. False: knockback away from origin.")]
	[SerializeReference]
	public BlackboardVariable<bool> KnockbackFromCenter = new BlackboardVariable<bool>(true);

	private bool _isComplete;
	private float _startTime;
	private float _duration;

	protected override Status OnStart()
	{
		PatternsController.Value.ShootMelee(
			Shape.Value,
			Width.Value,
			Height.Value,
			ArcAngle.Value,
			WarningTime.Value,
			ActiveTime.Value,
			SpawnDistance.Value,
			Offset.Value,
			Damage.Value,
			Transparency.Value,
			Origin.Value,
			Target.Value,
			TrackingLag.Value,
			KnockbackForce.Value,
			KnockbackFromCenter.Value,
			HitOnce.Value
		);

		if (!WaitForCompletion.Value)
		{
			return Status.Success;
		}

		_duration = WarningTime.Value + ActiveTime.Value + 0.2f; // +0.2 for end animation
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
