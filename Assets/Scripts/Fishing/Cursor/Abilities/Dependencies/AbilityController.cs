using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Stats;
using UnityEngine;
using UnityEngine.Events;

public class AbilityController : MonoBehaviour
{
	[FoldoutGroup("Debug")]
	[Tooltip("Whether to force a set of abilities. these abilites are applied on ResetAbilities().")]
	[SerializeField]
	private bool _abilityOverride;

	[FoldoutGroup("Debug")]
	[ShowIf("_abilityOverride")]
	[Tooltip("Whether the override applies on scene load.")]
	[SerializeField]
	private bool _applyOverrideOnLoad;

	[FoldoutGroup("Debug")]
	[ShowIf("_abilityOverride")]
	[SerializeField]
	private AbilitySO[] _abilityOverrides;

	[SerializeField]
	private AbilityDictionarySO _abilityDictionary;

	private readonly Dictionary<string, Ability> _runtimeAbilities = new();
	private readonly List<string> _abilitiesToRemove = new();
	private readonly Dictionary<Stats.StatType, float> _storedStats = new();

	private readonly UnityEvent _onAbilityPressed = new();
	private readonly UnityEvent _onAbilityReleased = new();

	[HideInInspector]
	public UnityEvent<Ability> OnAbilityAdded = new();

	[HideInInspector]
	public UnityEvent<Ability> OnAbilityRemoved = new();
	public IEnumerable<Ability> RuntimeAbilities => _runtimeAbilities.Values;

	private void OnEnable()
	{
		InputManager.Instance.OnAbilityPerformed.AddListener(InvokeAbilityPressed);
		InputManager.Instance.OnAbilityReleased.AddListener(InvokeAbilityReleased);

		if (_applyOverrideOnLoad)
		{
			ResetAbilities();
		}
	}

	private void OnDisable()
	{
		if (InputManager.Instance)
		{
			InputManager.Instance.OnAbilityPerformed.RemoveListener(InvokeAbilityPressed);
			InputManager.Instance.OnAbilityReleased.RemoveListener(InvokeAbilityReleased);
		}

		CleanUpAbilities();
	}

	private void FixedUpdate()
	{
		foreach (Ability ability in _runtimeAbilities.Values)
		{
			ability.Tick(Time.fixedDeltaTime);
		}

		CleanUpAbilities();
	}

	// Adds a specific ability
	[FoldoutGroup("Debug")]
	[Button]
	public void AddAbility(string id)
	{
		if (id.IsNullOrWhitespace())
		{
			return;
		}

		if (_runtimeAbilities.ContainsKey(id))
		{
			Debug.LogWarning($"Ability with id '{id}' is already registered");
			return;
		}

		AbilitySO abilitySO = _abilityDictionary.GetAbilitySOById(id);
		if (abilitySO == null)
		{
			Debug.LogError($"AbilitySO with id {id} not found");
			return;
		}

		Ability ability = abilitySO.CreateRuntimeInstance(gameObject);
		_runtimeAbilities.Add(id, ability);

		foreach (KeyValuePair<StatType, float> stat in _storedStats)
		{
			ability.SetStat(stat.Key, stat.Value);
		}

		// Listen to inputs
		_onAbilityPressed.AddListener(ability.OnButtonPressed);
		_onAbilityReleased.AddListener(ability.OnButtonReleased);

		OnAbilityAdded?.Invoke(ability);
	}

	// Removes a specific ability
	[FoldoutGroup("Debug")]
	[Button]
	public void RemoveAbility(string id)
	{
		_abilitiesToRemove.Add(id);
	}

	// Removes a specific ability
	[Button]
	public void RemoveAllAbilities()
	{
		foreach (string id in _runtimeAbilities.Keys)
		{
			_abilitiesToRemove.Add(id);
		}
		CleanUpAbilities();
	}

	// Removes and cleans up all runtime abilities
	private void CleanUpAbilities()
	{
		if (_abilitiesToRemove.IsNullOrEmpty())
		{
			return;
		}

		foreach (string id in _abilitiesToRemove)
		{
			if (_runtimeAbilities.TryGetValue(id, out Ability ability))
			{
				_onAbilityPressed.RemoveListener(ability.OnButtonPressed);
				_onAbilityReleased.RemoveListener(ability.OnButtonReleased);
				ability.CleanUp();
				OnAbilityRemoved?.Invoke(ability);
			}
			_runtimeAbilities.Remove(id);
		}
		_abilitiesToRemove.Clear();
	}

	public void ResetAbilities()
	{
		if (_abilityOverride)
		{
			RemoveAllAbilities();
			foreach (AbilitySO ability in _abilityOverrides)
			{
				AddAbility(ability.Id);
			}
		}

		foreach (Ability ability in _runtimeAbilities.Values)
		{
			ability.ResetAbility();
		}
	}

	public virtual void SetStat(StatType statType, float value)
	{
		_storedStats[statType] = value;
		foreach (Ability ability in _runtimeAbilities.Values)
		{
			ability.SetStat(statType, value);
		}
	}

	private void InvokeAbilityPressed() => _onAbilityPressed?.Invoke();

	private void InvokeAbilityReleased() => _onAbilityReleased?.Invoke();
}
