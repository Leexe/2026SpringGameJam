using UnityEngine;

namespace FishingGame.Abilities
{
	public abstract class AbilityUI : MonoBehaviour
	{
		public abstract void InstantiateUI(Ability ability);
		protected abstract void UpdateProgressUI(int charges, int maxCharges, float progress);
	}
}
