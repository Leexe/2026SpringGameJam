using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
	name: "FlashAction",
	story: "Play Flash Effect at [Target] for [FlashTime] seconds",
	category: "Action",
	id: "0f06a59fbb1592d5335dacd8c9129cf4"
)]
public partial class FlashAction : Action
{
	[SerializeReference]
	public BlackboardVariable<Transform> Target;

	[SerializeReference]
	public BlackboardVariable<float> FlashTime = (BlackboardVariable<float>)0.5f;

	[SerializeReference]
	public BlackboardVariable<GameObject> FlashPrefab;

	[SerializeReference]
	public BlackboardVariable<Vector3> Offset = (BlackboardVariable<Vector3>)Vector3.zero;

	private FlashEffect _flashEffect;
	private bool _isComplete;

	protected override Status OnStart()
	{
		if (FlashPrefab.Value == null)
		{
			Debug.LogError("FlashAction: FlashPrefab is null!");
			return Status.Failure;
		}

		if (Target.Value == null)
		{
			Debug.LogError("FlashAction: Target is null!");
			return Status.Failure;
		}

		_isComplete = false;

		Vector3 spawnPosition = Target.Value.position + Offset.Value;
		GameObject flash = UnityEngine.Object.Instantiate(FlashPrefab.Value, spawnPosition, Quaternion.identity);

		_flashEffect = flash.GetComponent<FlashEffect>();
		if (_flashEffect != null)
		{
			_flashEffect.Initialize(Target.Value, Offset.Value, FlashTime.Value);
			_flashEffect.OnComplete += HandleFlashComplete;
		}
		else
		{
			return Status.Success;
		}

		return Status.Running;
	}

	protected override Status OnUpdate()
	{
		return _isComplete ? Status.Success : Status.Running;
	}

	protected override void OnEnd()
	{
		if (_flashEffect != null)
		{
			_flashEffect.OnComplete -= HandleFlashComplete;
			_flashEffect = null;
		}
	}

	private void HandleFlashComplete()
	{
		_isComplete = true;
	}
}
