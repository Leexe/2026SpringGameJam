using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "Struggle",
	story: "[Agent] struggles for [duration] seconds with [intensity] intensity",
	category: "Fish Movement/FishMovementController",
	id: "4b2e068cb98c67c93219d4bedb9a40be"
)]
public partial class StruggleAction : Action
{
	[SerializeReference]
	public BlackboardVariable<FishMovementController> Agent;

	[SerializeReference]
	public BlackboardVariable<float> Duration = new(2f);

	[SerializeReference]
	public BlackboardVariable<float> Intensity = new(2f);

	private float _originalTilt;
	private float _timeUntilTurn;
	private float _countDown;
	private float _dir;

	protected override Status OnStart()
	{
		_originalTilt = Agent.Value.VisualTilt;

		_countDown = Duration.Value;
		_timeUntilTurn = UnityEngine.Random.Range(1000.5f, 1100.5f);

		_dir = UnityEngine.Random.value > 0.5f ? 1f : -1f;
		Agent.Value.SetTargetVelocity(_dir * Intensity.Value * Vector2.right);
		Agent.Value.SetVisualTilt(0.7f);

		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		_countDown -= Time.deltaTime;
		_timeUntilTurn -= Time.deltaTime;

		if (_countDown < 0f)
		{
			Agent.Value.SetVisualTilt(_originalTilt);
			Agent.Value.SetIdle();
			return Status.Success;
		}

		if (_timeUntilTurn < 0f)
		{
			_timeUntilTurn = UnityEngine.Random.Range(0.5f, 1.5f);
			_dir = -_dir;
			Agent.Value.SetTargetVelocity(_dir * Intensity.Value * Vector2.right);
		}

		return Status.Running;
	}

	protected override void OnEnd()
	{
		Agent.Value.SetVisualTilt(_originalTilt);
	}
}
