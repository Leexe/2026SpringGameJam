using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FishingGame.Abilities
{
	public class ChargeAbilityUI : AbilityUI
	{
		[SerializeField]
		private Image _progresImage;

		[SerializeField]
		private TextMeshProUGUI _chargeText;

		private ChargeAbility _chargeAbility;

		private void OnEnable()
		{
			_chargeAbility?.OnAbilityTick.AddListener(UpdateProgressUI);
			_chargeAbility?.OnAbilityActivate.AddListener(UpdateProgressUI);
		}

		private void OnDisable()
		{
			_chargeAbility?.OnAbilityTick.RemoveListener(UpdateProgressUI);
			_chargeAbility?.OnAbilityActivate.RemoveListener(UpdateProgressUI);
		}

		public override void InstantiateUI(Ability ability)
		{
			_chargeAbility = (ChargeAbility)ability;
			if (_chargeText == null)
			{
				Debug.LogWarning("ChargeAbilityUI is missing _chargeText reference");
			}
			_chargeAbility.OnAbilityTick.AddListener(UpdateProgressUI);
			UpdateProgressUI(
				_chargeAbility.CurrentCharges,
				_chargeAbility.MaxCharges,
				_chargeAbility.NormalizedProgress
			);

			// Enable the charge text only if the max charges > 1
			_chargeText.gameObject.SetActive(_chargeAbility.MaxCharges > 1);
		}

		protected override void UpdateProgressUI(int charges, int maxCharges, float progress)
		{
			if (_progresImage != null)
			{
				_progresImage.fillAmount = Mathf.Clamp((charges + progress) / maxCharges, 0f, 1f);
			}

			if (_chargeText != null)
			{
				_chargeText.text = $"{charges}";
			}
		}
	}
}
