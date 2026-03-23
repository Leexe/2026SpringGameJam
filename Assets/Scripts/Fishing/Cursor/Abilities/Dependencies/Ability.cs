using UnityEngine;

public abstract class Ability
{
	public AbilitySO AbilitySOParams => AbilitySO;
	protected bool ButtonHeldDown;
	protected AbilitySO AbilitySO;

	/// <summary>
	/// Constructor for ability instance
	/// </summary>
	/// <param name="abilitySO">Corresponding ability scriptable object</param>
	/// <param name="parent">Parent game object used to get script references</param>
	public Ability(AbilitySO abilitySO, GameObject parent)
	{
		AbilitySO = abilitySO;
	}

	/// <summary>
	/// The update method for an ability instance
	/// </summary>
	/// <param name="deltaTime">Change in time</param>
	public abstract void Tick(float deltaTime);

	/// <summary>
	/// Try to activate an ability, usually called on input action
	/// </summary>
	protected abstract void TryToTrigger();

	/// <summary>
	/// Performs some action when the corresponding keybind is pressed
	/// </summary>
	public virtual void OnButtonPressed()
	{
		ButtonHeldDown = true;
		if (AbilitySO.TypeOfInput == AbilitySO.InputType.Performed)
		{
			TryToTrigger();
		}
	}

	/// <summary>
	/// Performs some action when the corresponding keybind is released
	/// </summary>
	public virtual void OnButtonReleased()
	{
		ButtonHeldDown = false;
		if (AbilitySO.TypeOfInput == AbilitySO.InputType.Released)
		{
			TryToTrigger();
		}
		else if (AbilitySO.TypeOfInput == AbilitySO.InputType.HoldDown)
		{
			EndAbility();
		}
	}

	/// <summary>
	/// Activates an effect when ability ends
	/// </summary>
	protected virtual void EndAbility() { }

	/// <summary>
	/// Activates the ability, as opposed to trigger which tries to activate the ability
	/// </summary>
	/// <returns>Returns true if the ability was successfully activated and false if it wasn't</returns>
	protected virtual bool ActivateAbility()
	{
		return true;
	}

	/// <summary>
	/// Cleans up an ability, is called when an ability is removed from the ability controller
	/// </summary>
	public virtual void CleanUp() { }

	/// <summary>
	/// Resets ability values to their default, usually called on game restart
	/// </summary>
	public virtual void ResetAbility() { }

	/// <summary>
	/// Sets a stat for the ability
	/// </summary>
	/// <param name="statType">The stat to set</param>
	/// <param name="value">The new stat value</param>
	public virtual void SetStat(Stats.StatType statType, float value) { }
}
