using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace FishingGame.Abilities
{
	public class AbilityWithModifiers : Ability
	{
		private AbilityWithModifiersSO _abilitySO;
		private List<Modifier> _modifiers = new();

		/// <summary>
		/// Constructor for ability instance
		/// </summary>
		/// <param name="abilitySO">Corresponding ability scriptable object</param>
		/// <param name="parent">Parent game object used to get script references</param>
		public AbilityWithModifiers(AbilityWithModifiersSO abilitySO, GameObject parent)
			: base(abilitySO, parent)
		{
			_abilitySO = abilitySO;
			ActivateAbility();
		}

		// Adds modifiers from the scriptable object
		protected override bool ActivateAbility()
		{
			foreach (Modifier modifier in _abilitySO.Modifiers)
			{
				Modifier newModifier = GameManager.Instance.PlayerStats.AddModifier(
					modifier.StatName,
					modifier.FlatAdd,
					modifier.PercentIncreaseAdd,
					modifier.ModifierMult,
					modifier.FinalMult
				);

				_modifiers.Add(newModifier);
			}
			return true;
		}

		public override void CleanUp()
		{
			RemoveAbilityModifiers();
		}

		// Removes modifiers
		private void RemoveAbilityModifiers()
		{
			foreach (Modifier modifier in _modifiers)
			{
				GameManager.Instance.PlayerStats.RemoveModifier(modifier);
			}
			_modifiers.Clear();
		}

		public override void OnButtonPressed() { }

		public override void OnButtonReleased() { }

		public override void Tick(float deltaTime) { }

		protected override void TryToTrigger() { }

		//public override void ResetAbility() { }
	}
}
