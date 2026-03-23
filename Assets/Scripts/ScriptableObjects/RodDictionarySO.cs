using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "RodDictionarySO", menuName = "Dictionaries/RodDictionarySO")]
public class RodDictionarySO : SerializedScriptableObject
{
	[Header("Dictionaries")]
	[OdinSerialize, ReadOnly]
	public Dictionary<string, RodSO> RodSODict { get; private set; }

	[OdinSerialize, ReadOnly]
	public Dictionary<string, RodAttachmentSO> AttachmentSODict { get; private set; }

	/// <summary>
	/// Look up a RodSO by string ID.
	/// Primarily used when loading from a save file.
	/// </summary>
	public RodSO GetRodSOById(string id)
	{
		if (RodSODict != null && RodSODict.TryGetValue(id, out RodSO rod))
		{
			return rod;
		}

		Debug.LogError($"Rod with ID '{id}' not found in dictionary.");
		return null;
	}

	/// <summary>
	/// Look up a RodSO by string ID.
	/// Primarily used when loading from a save file.
	/// </summary>
	public RodAttachmentSO GetRodAttachmentSOById(string id)
	{
		if (AttachmentSODict != null && AttachmentSODict.TryGetValue(id, out RodAttachmentSO rodAttachment))
		{
			return rodAttachment;
		}

		Debug.LogError($"Rod with ID '{id}' not found in dictionary.");
		return null;
	}

#if UNITY_EDITOR
	[Button]
	[PropertyOrder(-1)]
	[Tooltip("Autogenerate SODict from SOs in the ScriptableObjects/Rods and ScriptableObjects/RodAttachments folder")]
	[UsedImplicitly]
	private void GenerateSODict()
	{
		RodSODict = SODictionaryUtility.GenerateSODict<RodSO>("Assets/ScriptableObjects/Rods");
		AttachmentSODict = SODictionaryUtility.GenerateSODict<RodAttachmentSO>(
			"Assets/ScriptableObjects/RodAttachments"
		);
		UnityEditor.EditorUtility.SetDirty(this);
	}
#endif
}
