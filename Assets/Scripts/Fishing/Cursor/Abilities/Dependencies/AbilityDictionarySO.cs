using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDictionarySO", menuName = "Dictionaries/AbilityDictionarySO")]
public class AbilityDictionarySO : SerializedScriptableObject
{
	[OdinSerialize, ReadOnly]
	public Dictionary<string, AbilitySO> SODict { get; private set; }

	/// <summary>
	/// Look up a AbilitySO by string ID.
	/// Primarily used to match AbilityInstances with their AbilitySOs when loading from a save file.
	/// </summary>
	public AbilitySO GetAbilitySOById(string id)
	{
		if (SODict != null && SODict.TryGetValue(id, out AbilitySO ability))
		{
			return ability;
		}

		Debug.LogError($"Ability with ID '{id}' not found in dictionary.");
		return null;
	}

#if UNITY_EDITOR
	[Button]
	[PropertyOrder(-1)]
	[Tooltip("Autogenerate SODict from SOs in the ScriptableObjects/Abilities folder")]
	[UsedImplicitly]
	private void GenerateSODict()
	{
		SODict = SODictionaryUtility.GenerateSODict<AbilitySO>("Assets/ScriptableObjects/Abilities");
		UnityEditor.EditorUtility.SetDirty(this);
	}
#endif
}
