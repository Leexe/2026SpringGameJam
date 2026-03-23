using System;
using UnityEngine;

public class CursorController : MonoBehaviour
{
	[Header("References")]
	[field: SerializeField]
	public CursorMovementController Movement { get; private set; }

	[field: SerializeField]
	public Damageable Damageable { get; private set; }

	[field: SerializeField]
	public GenericAttack GenericAttack { get; private set; }

	[field: SerializeField]
	public HealthController Health { get; private set; }

	[field: SerializeField]
	public AbilityController Ability { get; private set; }

	public event Action OnLose;

	//

	public CursorState State { get; private set; }

	//

	private void OnEnable()
	{
		Damageable.OnDamaged += OnDamage;
		Health.OnDeath.AddListener(OnDeath);

		InputManager.Instance.OnAnchorPerformed.AddListener(HandleAnchorPerformed);
		InputManager.Instance.OnAnchorReleased.AddListener(HandleAnchorReleased);
	}

	private void OnDisable()
	{
		Damageable.OnDamaged -= OnDamage;

		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnAnchorPerformed.RemoveListener(HandleAnchorPerformed);
			InputManager.Instance.OnAnchorReleased.RemoveListener(HandleAnchorReleased);
		}
	}

	//

	private void HandleAnchorPerformed()
	{
		GenericAttack.SetEnabled(true);
	}

	private void HandleAnchorReleased()
	{
		GenericAttack.SetEnabled(false);
	}

	//

	public void BeginCombat(FishController fish)
	{
		State = CursorState.Fighting;
		Movement.UnGrab();
		Movement.Launch(Vector2.up * 5f);
		Health.Revive();
		Damageable.SetDamageable(true);
		GenericAttack.SetEnabled(false);
	}

	public void ReturnToHook(HookController hook)
	{
		State = CursorState.Gone;
		Movement.Grab(hook.transform);
		Health.Revive();
		Damageable.SetDamageable(false);
		GenericAttack.SetEnabled(false);
	}

	private void OnDamage(DamageData data)
	{
		if (data.HasKnockback)
		{
			Movement.Launch(data.KnockbackVector, true);
		}
	}

	private void OnDeath()
	{
		if (State != CursorState.Fighting)
		{
			Debug.LogError("Cursor lost game when wasn't fighting");
			return;
		}

		State = CursorState.Gone;
		OnLose?.Invoke();
	}

	public enum CursorState
	{
		Gone,
		Fighting,
	}
}
