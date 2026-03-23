using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Rod", menuName = "Rod/RodSO")]
public class RodSO : ScriptableObject
{
	[field: Title("Basic Info")]
	[field: Tooltip("Unique string id. Primarily used to identify the SO when serializing")]
	[field: SerializeField]
	public string Id { get; private set; }

	[field: Tooltip("The name of the rod")]
	[field: SerializeField]
	public string Name { get; private set; }

	[field: Tooltip("A short description of the rod")]
	[field: SerializeField]
	[field: TextArea(4, 6)]
	public string Description { get; private set; } = "This is a Rod";

	[field: Tooltip("Sprite to be shown in the inventory")]
	[field: SerializeField]
	[field: PreviewField(100, ObjectFieldAlignment.Right)]
	public Sprite Sprite { get; private set; }

	// should this be a prefab?
	[field: Tooltip("Sprite to be shown during gameplay")]
	[field: SerializeField]
	[field: PreviewField(100, ObjectFieldAlignment.Right)]
	public Sprite RodSprite { get; private set; }

	[field: Title("Stats")]
	[field: Tooltip("How strong your rod is - influences minigame donut size. Weak rod -> reduced donut size")]
	[field: SerializeField]
	[field: Range(0f, 100f)]
	public float Stability { get; private set; } = 10f;

	[field: Tooltip("How much 'HP' you have during the fishing minigame")]
	[field: SerializeField]
	[field: Range(0f, 1000f)]
	public float LineStrength { get; private set; } = 100f;

	[field: Tooltip("How far down the hook can go when you cast it (units)")]
	[field: SerializeField]
	[field: Range(0f, 100f)]
	public float LineLength { get; private set; } = 10f;
}
