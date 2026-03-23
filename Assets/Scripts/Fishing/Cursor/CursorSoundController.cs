using FMOD.Studio;
using UnityEngine;

public class CursorSoundController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private CursorController _cursor;

	[Header("Parameters")]
	[SerializeField]
	private float _minimumDamageForSound = 3f;

	// Private Variables
	private EventInstance _reelLoopSFX;

	private void Start()
	{
		// _reelLoopSFX = AudioManager.Instance.CreateInstance(FMODEvents.Instance.Reel_LoopSfx);
	}

	private void OnEnable()
	{
		_cursor.Damageable.OnDamaged += HandleDamage;
		_cursor.GenericAttack.OnDamageEnable += HandleAttackStart;
		_cursor.GenericAttack.OnDamageDisable += HandleAttackEnd;
	}

	private void OnDisable()
	{
		if (_cursor)
		{
			_cursor.Damageable.OnDamaged -= HandleDamage;
			_cursor.GenericAttack.OnDamageEnable -= HandleAttackStart;
			_cursor.GenericAttack.OnDamageDisable -= HandleAttackEnd;
		}
	}

	private void OnDestroy()
	{
		if (AudioManager.Instance)
		{
			AudioManager.Instance.DestroyInstance(_reelLoopSFX);
		}
	}

	private void HandleDamage(DamageData damageData)
	{
		if (damageData.Amount > _minimumDamageForSound)
		{
			// AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Struggle_Sfx);
		}
	}

	/* Reel Loop SFX */

	private void HandleAttackStart()
	{
		if (_cursor.State == CursorController.CursorState.Fighting)
		{
			AudioManager.Instance.PlayInstance(_reelLoopSFX);
		}
	}

	private void HandleAttackEnd()
	{
		AudioManager.Instance.StopInstance(_reelLoopSFX);
	}
}
