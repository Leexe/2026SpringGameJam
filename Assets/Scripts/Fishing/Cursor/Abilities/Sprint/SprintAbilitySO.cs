using Sirenix.OdinInspector;
using UnityEngine;

namespace FishingGame.Abilities
{
	[CreateAssetMenu(fileName = "Sprint", menuName = "Abilities/Sprint", order = 0)]
	public class SprintAbilitySO : ChargeAbilitySO
	{
		[field: TabGroup("Sprint")]
		[field: SerializeField]
		public float SprintMult { get; private set; } = 1.5f;

		public override Ability CreateRuntimeInstance(GameObject parent)
		{
			return new SprintAbility(this, parent);
		}
	}
}
