using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "FishDictionarySO", menuName = "Dictionaries/FishDictionarySO")]
public class FishDictionarySO : SerializedScriptableObject
{
	[OdinSerialize, ReadOnly]
	public Dictionary<string, FishSO> SODict { get; private set; }

	/// <summary>
	/// Look up a FishSO by string ID.
	/// Primarily used to match FishInstances with their FishSOs when loading from a save file.
	/// </summary>
	public FishSO GetFishSOById(string id)
	{
		if (SODict != null && SODict.TryGetValue(id, out FishSO fish))
		{
			return fish;
		}

		Debug.LogError($"Fish with ID '{id}' not found in dictionary.");
		return null;
	}

#if UNITY_EDITOR
	[Button]
	[PropertyOrder(-1)]
	[Tooltip("Autogenerate SODict from SOs in the ScriptableObjects/Fish folder")]
	[UsedImplicitly]
	private void GenerateSODict()
	{
		SODict = SODictionaryUtility.GenerateSODict<FishSO>("Assets/ScriptableObjects/Fish");
		UnityEditor.EditorUtility.SetDirty(this);
	}
#endif
}
