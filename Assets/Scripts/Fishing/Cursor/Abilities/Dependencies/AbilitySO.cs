using Sirenix.OdinInspector;
using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
	public enum InputType
	{
		None,
		Performed,
		HoldDown,
		Released,
	}

	[field: TabGroup("Basic Data")]
	[field: SerializeField]
	[field: Required]
	[field: Tooltip("The id of the ability for the purposes of mapping")]
	public string Id { get; private set; }

	[field: TabGroup("Basic Data")]
	[field: SerializeField]
	[field: Required]
	[field: Tooltip("The name of the ability")]
	public string Name { get; private set; }

	[field: TabGroup("Basic Data")]
	[field: SerializeField]
	[field: Tooltip("Prefab of the UI Element")]
	public GameObject UIPrefab { get; private set; }

	[field: TabGroup("Basic Data")]
	[field: SerializeField]
	[field: Tooltip("")]
	public InputType TypeOfInput { get; private set; } = InputType.Performed;

	/// <summary>
	/// Instantiates an Ability class from this AbilitySO
	/// </summary>
	/// <param name="parent"></param>
	/// <returns></returns>
	public abstract Ability CreateRuntimeInstance(GameObject parent);
}
