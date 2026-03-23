using Sirenix.OdinInspector;
using UnityEngine;

namespace FishingGame.Abilities
{
	public abstract class ChargeAbilitySO : AbilitySO
	{
		[field: Title("Charges")]
		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("How many uses an ability has at at any one time")]
		public int MaxCharges { get; private set; } = 2;

		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("How long it takes for one charge to recharge")]
		public float Cooldown { get; private set; } = 3;

		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip(
			"How long it takes for a charge to decay while ability is active, set to 0 if charges don't decay while active"
		)]
		public int ActiveChargeDecay { get; private set; }

		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("The amount of charges consumed when ability is activated")]
		public int ChargesConsumedOnPerformed { get; private set; } = 1;

		[field: Title("Cooldowns")]
		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("How long it takes after an ability activates before it starts recharging")]
		public float CooldownFreeze { get; private set; } = 0.5f;

		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("How long an ability lasts while active")]
		public float ActiveDuration { get; private set; } = 3;

		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("How long after activating an ability before being allowed to reactivate it")]
		public float ActionDelay { get; private set; } = 0.2f;

		[field: TabGroup("Charge Ability Data")]
		[field: SerializeField]
		[field: Tooltip("Does the ability recharge while it is active")]
		public bool RechargeWhileActive { get; private set; }
	}
}
