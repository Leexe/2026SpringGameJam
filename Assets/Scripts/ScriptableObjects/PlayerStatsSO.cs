using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Stats;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", order = 0)]
public class PlayerStatsSO : SerializedScriptableObject
{
	[ShowInInspector]
	[OdinSerialize]
	[DictionaryDrawerSettings(KeyLabel = "Stat", ValueLabel = "Base Value")]
	public Dictionary<StatType, float> BaseStatsMap { get; private set; }

#if UNITY_EDITOR
	[Button("Generate")]
	[PropertyOrder(-1)]
	[Tooltip("Add any missing StatType enum values to the dictionary with a default value of 0")]
	[UsedImplicitly]
	private void SyncWithEnum()
	{
		foreach (StatType statType in Enum.GetValues(typeof(StatType)))
		{
			if (BaseStatsMap.TryAdd(statType, 0f))
			{
				Debug.Log($"Added missing stat: {statType} with default value 0");
			}
		}
		UnityEditor.EditorUtility.SetDirty(this);
	}
#endif
}
