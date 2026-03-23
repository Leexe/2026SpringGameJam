using System.Collections.Generic;
using UnityEngine;

namespace FishingGame.Abilities
{
	public class AbilityUIController : MonoBehaviour
	{
		[SerializeField]
		private AbilityController _abilityController;

		[SerializeField]
		private GameObject _abilitiesRoot;

		private readonly Dictionary<string, AbilityUI> _activeAbilities = new();

		private void OnEnable()
		{
			_abilityController.OnAbilityAdded.AddListener(AddAbility);
			_abilityController.OnAbilityRemoved.AddListener(DisableAbility);
		}

		private void OnDisable()
		{
			if (_abilityController)
			{
				_abilityController.OnAbilityAdded.RemoveListener(AddAbility);
				_abilityController.OnAbilityRemoved.RemoveListener(DisableAbility);
			}
		}

		private void AddAbility(Ability ability)
		{
			AbilitySO abilitySO = ability.AbilitySOParams;

			if (abilitySO.UIPrefab == null)
			{
				return;
			}

			if (!_activeAbilities.ContainsKey(abilitySO.Id))
			{
				GameObject instance = Instantiate(abilitySO.UIPrefab, _abilitiesRoot.transform);

				if (!instance.TryGetComponent(out AbilityUI abilityUI))
				{
					Destroy(instance);
					Debug.LogError($"AbilityUI not found on {abilitySO.Id} prefab");
					return;
				}

				_activeAbilities.TryAdd(abilitySO.Id, abilityUI);
			}

			_activeAbilities[abilitySO.Id].InstantiateUI(ability);
			_activeAbilities[abilitySO.Id].gameObject.SetActive(true);
		}

		private void DisableAbility(Ability ability)
		{
			AbilitySO abilitySO = ability.AbilitySOParams;
			if (!_activeAbilities.ContainsKey(abilitySO.Id))
			{
				return;
			}
			_activeAbilities[abilitySO.Id].gameObject.SetActive(false);
		}
	}
}
