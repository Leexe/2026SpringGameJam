using UnityEngine;

namespace FishingGame.Abilities
{
	public class DashAbility : ChargeAbility
	{
		private readonly CursorController _playerController;
		private readonly CursorVFXController _vfxController;
		private readonly DashAbilitySO _abilitySO;

		private CursorMovementController _movementController;
		private Vector2 _dashDir;
		private float _timer = 0f;

		public DashAbility(DashAbilitySO abilitySO, GameObject parent)
			: base(abilitySO, parent)
		{
			_abilitySO = abilitySO;
			_playerController = parent.GetComponent<CursorController>();
			_vfxController = parent.GetComponent<CursorVFXController>();
			_movementController = _playerController.Movement;
		}

		protected override bool ActivateAbility()
		{
			base.ActivateAbility();
			_vfxController.StartSpeedLine();
			// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Dash_Sfx, _playerController.transform.position);

			if (_playerController == null || _movementController == null)
			{
				return false;
			}
			if (_movementController.State != CursorMovementController.MovementState.Controlled)
			{
				return false;
			}

			_dashDir = _movementController.LastNonZeroMovementVector.normalized;
			_timer = _abilitySO.ActiveDuration;

			_movementController.KinematicMode();
			_movementController.Rb.linearVelocity = _abilitySO.DashForce * _movementController.BaseMoveSpeed * _dashDir;

			_playerController.Health.SetInvincibilityTime(_abilitySO.InvincibilityDuration);

			return true;
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);

			if (IsActive)
			{
				_timer -= deltaTime;
				if (_timer <= 0f)
				{
					EndAbility();
				}
			}
		}

		protected override void EndAbility()
		{
			base.EndAbility();

			_movementController.Rb.linearVelocity = Vector2.zero;
			_movementController.ControlledMode();
			// bit of a movement control penalty after you exit the dash
			// _movementController.Launch(_abilitySO.DashForce * 0.2f * _dashDir);

			_vfxController.StopSpeedLine();
		}
	}
}
