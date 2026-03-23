using Sirenix.OdinInspector;
using UnityEngine;

public enum RodAttachmentType
{
	Trinket,
	Bobber,
}

[CreateAssetMenu(fileName = "RodAttachmentSO", menuName = "Rod/RodAttachmentSO")]
public class RodAttachmentSO : ScriptableObject
{
	[field: Title("Basic Info")]
	[field: Tooltip("Unique string id. Primarily used to identify the SO when serializing")]
	[field: Required]
	[field: SerializeField]
	public string Id { get; private set; }

	[field: Tooltip("The name of the attachment")]
	[field: SerializeField]
	public string Name { get; private set; }

	[field: Tooltip("The type of the attachment")]
	[field: SerializeField]
	public RodAttachmentType AttachmentType { get; private set; } = RodAttachmentType.Bobber;

	[field: Tooltip("A short description of the attachment")]
	[field: SerializeField]
	[field: TextArea(5, 8)]
	public string Description { get; private set; } = "This is either a bobber or a charm.";

	// should this be a prefab?
	[field: Tooltip("Sprite to be shown during gameplay")]
	[field: SerializeField]
	[field: PreviewField(100, ObjectFieldAlignment.Right)]
	public Sprite AttachmentSprite { get; private set; }

	[field: Tooltip("The ability associated with this attachment")]
	[field: SerializeField]
	public AbilitySO AbilitySO { get; private set; } = null;
}
