using Sirenix.OdinInspector;
using UnityEngine;

namespace FishingGame.Abilities
{
	[CreateAssetMenu(fileName = "Dash", menuName = "Abilities/Dash", order = 0)]
	public class DashAbilitySO : ChargeAbilitySO
	{
		[field: TabGroup("Dash")]
		[field: SerializeField]
		public float DashForce { get; private set; } = 5f;

		[field: TabGroup("Dash")]
		[field: SerializeField]
		public float InvincibilityDuration { get; private set; } = 0.5f;

		public override Ability CreateRuntimeInstance(GameObject parent)
		{
			return new DashAbility(this, parent);
		}
	}
}
