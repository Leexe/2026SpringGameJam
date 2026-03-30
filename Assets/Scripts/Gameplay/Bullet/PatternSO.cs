using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "Gameplay/Pattern")]
public class PatternSO : ScriptableObject
{
	[SerializeField]
	[ListDrawerSettings(ShowFoldout = true)]
	private List<AttackEntry> _attacks = new();

	public List<AttackEntry> Attacks => _attacks;
}

[Serializable]
public struct AttackEntry
{
	[Required]
	[Tooltip("The attack to fire")]
	public AttackSO Attack;

	[Tooltip("Time to wait after firing this attack (seconds)")]
	[Min(0f)]
	public float DelayAfter;
}
