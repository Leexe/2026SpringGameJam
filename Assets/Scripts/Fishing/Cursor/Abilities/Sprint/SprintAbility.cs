using FMOD.Studio;
using UnityEngine;

namespace FishingGame.Abilities
{
	public class SprintAbility : ChargeAbility
	{
		private readonly CursorController _playerController;
		private readonly CursorVFXController _vfxController;
		private readonly SprintAbilitySO _abilitySO;
		private bool _sprinting;
		private EventInstance _sprintSfxInstance;

		public SprintAbility(SprintAbilitySO abilitySO, GameObject parent)
			: base(abilitySO, parent)
		{
			_abilitySO = abilitySO;
			_playerController = parent.GetComponent<CursorController>();
			_vfxController = parent.GetComponent<CursorVFXController>();

			// _sprintSfxInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.Sprint_LoopSfx, parent);
		}

		protected override bool ActivateAbility()
		{
			base.ActivateAbility();

			_playerController.Movement.SetMovementSpeedMult(_abilitySO.SprintMult);
			if (!_sprinting)
			{
				_vfxController.StartSprintTrail();
				AudioManager.Instance.PlayInstanceAtStart(_sprintSfxInstance);
			}
			_sprinting = true;
			return true;
		}

		protected override void EndAbility()
		{
			base.EndAbility();
			_vfxController.StopSprintTrail();
			_playerController.Movement.SetMovementSpeedMult(1f);
			if (_sprinting)
			{
				AudioManager.Instance.StopInstance(_sprintSfxInstance, true);
			}
			_sprinting = false;
		}

		public override void CleanUp()
		{
			EndAbility();
		}
	}
}
