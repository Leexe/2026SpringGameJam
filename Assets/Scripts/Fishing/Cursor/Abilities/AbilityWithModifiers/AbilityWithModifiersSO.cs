using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

namespace FishingGame.Abilities
{
	[CreateAssetMenu(fileName = "WithModifiers", menuName = "Abilities/WithModifiers", order = 0)]
	public class AbilityWithModifiersSO : AbilitySO
	{
		[field: TabGroup("Modifiers")]
		[field: SerializeField]
		[field: Tooltip(
			"If you're not using flatAdd or percentIncreaseAdd, set them to 0. If you're not using modifierMult or finalMult, set them to 1."
		)]
		public List<Modifier> Modifiers { get; private set; }

		public override Ability CreateRuntimeInstance(GameObject parent)
		{
			return new AbilityWithModifiers(this, parent);
		}
	}
}
