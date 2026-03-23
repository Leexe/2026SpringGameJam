using UnityEngine;

namespace FishingGame.Abilities
{
	public class ShockwaveAbility : ChargeAbility
	{
		private readonly GameObject _playerGameObject;
		private readonly ShockwaveAbilitySO _abilitySO;
		private readonly GameObject _shockwavePrefab;

		public ShockwaveAbility(ShockwaveAbilitySO abilitySO, GameObject parent)
			: base(abilitySO, parent)
		{
			_abilitySO = abilitySO;
			_shockwavePrefab = abilitySO.ShockwavePrefab;
			_playerGameObject = parent;
		}

		protected override bool ActivateAbility()
		{
			base.ActivateAbility();

			GameObject shockwaveInstance = UnityEngine.Object.Instantiate(
				_shockwavePrefab,
				_playerGameObject.transform.position,
				Quaternion.identity
			);
			ShockwaveController shockwaveController = shockwaveInstance.GetComponent<ShockwaveController>();
			shockwaveController.Initialize(
				_abilitySO.StartRadius,
				_abilitySO.EndRadius,
				_abilitySO.StartDelay,
				_abilitySO.Duration,
				_abilitySO.FishLayer,
				_abilitySO.BulletTag,
				_playerGameObject
			);

			return true;
		}
	}
}
