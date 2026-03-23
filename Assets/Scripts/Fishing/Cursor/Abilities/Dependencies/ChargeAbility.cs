using UnityEngine;
using UnityEngine.Events;

namespace FishingGame.Abilities
{
	public abstract class ChargeAbility : Ability
	{
		private ChargeAbilitySO _abilitySO;
		private int _charges;
		private bool _isActive;
		private float _cooldownReduction;
		private bool _statsChanged;

		// Default Cooldowns
		private float _cooldown;
		private float _cooldownFreeze;
		private float _activeDuration;
		private float _actionDelay;
		private float _activeChargeDecay;

		// Timers
		private float _cooldownTimer;
		private float _cooldownFreezeTimer;
		private float _activeDurationTimer;
		private float _actionDelayTimer;

		// Events
		public UnityEvent<int, int, float> OnAbilityActivate = new(); // Charges, Max Charges, Normalized Progress
		public UnityEvent<int, int, float> OnAbilityTick = new(); // Charges, Max Charges, Normalized Progress
		public UnityEvent<int, int, float> OnAbilityRecharged = new(); // Charges, Max Charges, Normalized Progress
		public UnityEvent OnAbilityFailedTrigger = new();
		public UnityEvent OnAbilityEnd = new();

		public bool IsActive => _isActive;
		public int CurrentCharges => _charges;
		public int MaxCharges => _abilitySO.MaxCharges;
		public float CooldownTimer => _cooldownTimer;
		public float MaxCooldown => _cooldown;
		public float NormalizedProgress => _cooldown > 0 ? Mathf.Clamp(1 - (_cooldownTimer / _cooldown), 0, 1) : 1f;

		public ChargeAbility(ChargeAbilitySO abilitySO, GameObject parent)
			: base(abilitySO, parent)
		{
			_charges = abilitySO.MaxCharges;
			_abilitySO = abilitySO;
			ApplyStatChanges();
		}

		public override void Tick(float deltaTime)
		{
			// Handle On Hold Events
			if (_abilitySO.TypeOfInput == AbilitySO.InputType.HoldDown && ButtonHeldDown)
			{
				TryToTrigger();
			}

			// Handle Stat Changes
			if (_statsChanged)
			{
				ApplyStatChanges();
			}

			// Handle Action Delay Time
			if (_actionDelayTimer > 0f)
			{
				_actionDelayTimer -= deltaTime;
			}

			// Handle Cooldown Freeze Timer
			if (_cooldownFreezeTimer > 0f)
			{
				_cooldownFreezeTimer -= deltaTime;
			}

			// Handle Charges
			if (
				_charges < _abilitySO.MaxCharges
				&& _cooldownFreezeTimer <= 0
				&& (!_isActive || _abilitySO.RechargeWhileActive)
			)
			{
				_cooldownTimer -= deltaTime;
				if (_cooldownTimer <= 0f)
				{
					_charges++;
					_cooldownTimer = 0f;
					if (_charges < _abilitySO.MaxCharges)
					{
						_cooldownTimer = _cooldown;
					}
					OnAbilityRecharged?.Invoke(_charges, _abilitySO.MaxCharges, NormalizedProgress);
				}
			}

			// Handle Active Time
			if (_isActive)
			{
				_activeDurationTimer -= deltaTime;

				if (_activeDurationTimer <= 0f)
				{
					EndAbility();
				}
				else if (_activeChargeDecay > 0f)
				{
					_cooldownTimer += _cooldown / _activeChargeDecay * deltaTime;

					if (_charges == 0 && _cooldownTimer >= _cooldown)
					{
						EndAbility();
					}
					else if (_cooldownTimer >= _cooldown)
					{
						_charges--;
						_cooldownTimer -= _cooldown;
					}
				}
			}

			OnAbilityTick?.Invoke(_charges, _abilitySO.MaxCharges, NormalizedProgress);
		}

		public override void ResetAbility()
		{
			_charges = _abilitySO.MaxCharges;
			_cooldownTimer = 0f;
			_cooldownFreezeTimer = 0f;
			ButtonHeldDown = false;
			_activeDurationTimer = 0f;
			_actionDelayTimer = 0f;
			_isActive = false;
		}

		protected override void TryToTrigger()
		{
			// Activate only if there are enough charges
			if (
				_charges < _abilitySO.ChargesConsumedOnPerformed
				|| _actionDelayTimer > 0f
				|| (_abilitySO.ChargesConsumedOnPerformed == 0 && NormalizedProgress <= 0.01f)
			)
			{
				return;
			}

			if (ActivateAbility() == false)
			{
				OnAbilityFailedTrigger?.Invoke();
				return;
			}

			if (_statsChanged)
			{
				ApplyStatChanges();
			}

			if (_charges == _abilitySO.MaxCharges)
			{
				_cooldownTimer = _cooldown;
			}

			_cooldownFreezeTimer = _cooldownFreeze;
			_actionDelayTimer = _actionDelay;
			_activeDurationTimer = _activeDuration;
			_charges = Mathf.Max(0, _charges - _abilitySO.ChargesConsumedOnPerformed);
			_isActive = true;
			OnAbilityActivate?.Invoke(_charges, _abilitySO.MaxCharges, NormalizedProgress);
		}

		protected override void EndAbility()
		{
			OnAbilityEnd?.Invoke();
			_activeDurationTimer = 0f;
			_isActive = false;
		}

		protected virtual void ApplyStatChanges()
		{
			float oldCooldown = _cooldown;
			_cooldown = _abilitySO.Cooldown * (1 - (_cooldownReduction / 100f));

			if (oldCooldown > 0f && _cooldownTimer > 0f)
			{
				_cooldownTimer = _cooldownTimer / oldCooldown * _cooldown;
			}

			_cooldownFreeze = _abilitySO.CooldownFreeze;
			_activeDuration = _abilitySO.ActiveDuration;
			_actionDelay = _abilitySO.ActionDelay;
			_activeChargeDecay = _abilitySO.ActiveChargeDecay;
			_statsChanged = false;
		}

		public override void SetStat(Stats.StatType statType, float value)
		{
			switch (statType)
			{
				case Stats.StatType.CooldownReduction:
					_statsChanged = true;
					_cooldownReduction = value;
					ApplyStatChanges();
					break;
			}
		}
	}
}
