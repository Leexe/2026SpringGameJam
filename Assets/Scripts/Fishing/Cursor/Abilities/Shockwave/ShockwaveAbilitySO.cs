using Sirenix.OdinInspector;
using UnityEngine;

namespace FishingGame.Abilities
{
	[CreateAssetMenu(fileName = "Shockwave", menuName = "Abilities/Shockwave", order = 0)]
	public class ShockwaveAbilitySO : ChargeAbilitySO
	{
		[field: TabGroup("Shockwave")]
		[field: Tooltip("How big the circle is initially, before collapsing and expanding again")]
		[field: SerializeField]
		public float StartRadius { get; private set; } = 1f;

		[field: TabGroup("Shockwave")]
		[field: Tooltip("The fully expanded radius of the bullet deleting circle")]
		[field: SerializeField]
		public float EndRadius { get; private set; } = 5f;

		[field: TabGroup("Shockwave")]
		[field: Tooltip("The time it takes for the initial area to collapse and the shockwave to appear")]
		[field: SerializeField]
		public float StartDelay { get; private set; } = 0.5f;

		[field: TabGroup("Shockwave")]
		[field: Tooltip("The time it takes for the area to reach full expansion")]
		[field: SerializeField]
		public float Duration { get; private set; } = 3f;

		[field: TabGroup("Shockwave")]
		[field: SerializeField]
		public LayerMask FishLayer { get; private set; }

		[field: TabGroup("Shockwave")]
		[field: SerializeField]
		public string BulletTag { get; private set; } = "Bullet";

		[field: TabGroup("Shockwave")]
		[field: Tooltip("The prefab for the shockwave")]
		[field: SerializeField]
		public GameObject ShockwavePrefab { get; private set; }

		public override Ability CreateRuntimeInstance(GameObject parent)
		{
			return new ShockwaveAbility(this, parent);
		}
	}
}
