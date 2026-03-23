using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// CURRENTLY UNUSED.
// will be useful for the minigame down the line, when there are possibly multiple things for a skill or attack to target.

/// <summary>
/// A MonoBehaviour script for anything that can target Targetable objects.
/// Provides functionality to query nearby targets, respecting targetability and tangibility.
/// </summary>
public class TargetingController : MonoBehaviour
{
	[Header("Parameters (optional)")]
	[Tooltip("Whether targeting requires tangibility check")]
	[SerializeField]
	private bool _requireTangible = false;

	[Tooltip("Filter targets by category (None = no filter)")]
	[SerializeField]
	private CombatTeam _categoryFilter = CombatTeam.None;

	public bool RequireTangible
	{
		get => _requireTangible;
		set => _requireTangible = value;
	}

	public CombatTeam CategoryFilter
	{
		get => _categoryFilter;
		set => _categoryFilter = value;
	}

	public Targetable CurrentTarget { get; private set; }
	public IReadOnlyCollection<Targetable> ActiveTargets => _activeTargets;

	public event Action<Targetable> OnTargeted;
	public event Action<Targetable> OnUntargeted;

	private readonly HashSet<Targetable> _activeTargets = new();

	/// <summary>
	/// Sets the current target, automatically untargeting the previous one if it exists.
	/// </summary>
	public bool SetTarget(Targetable targetable)
	{
		if (targetable == CurrentTarget)
		{
			return true;
		}

		// Untarget current
		if (CurrentTarget != null)
		{
			ClearTarget();
		}

		// Target new
		if (targetable != null)
		{
			if (!IsValidTarget(targetable))
			{
				Debug.LogWarning($"Attempted to target invalid targetable: {targetable.name}");
				return false;
			}

			if (!targetable.Target(this))
			{
				Debug.LogWarning($"Targetable {targetable.name} refused targeting");
				return false;
			}

			CurrentTarget = targetable;
			_activeTargets.Add(targetable);
			OnTargeted?.Invoke(targetable);
			return true;
		}

		return true;
	}

	/// <summary>
	/// Clears the current target.
	/// </summary>
	public bool ClearTarget()
	{
		if (CurrentTarget == null)
		{
			return false;
		}

		Targetable previous = CurrentTarget;
		CurrentTarget = null;
		_activeTargets.Remove(previous);
		previous.UnTarget(this);
		OnUntargeted?.Invoke(previous);
		return true;
	}

	/// <summary>
	/// Adds a target to the active targets set without setting it as CurrentTarget.
	/// Useful for multi-target scenarios.
	/// </summary>
	public bool AddTarget(Targetable targetable)
	{
		if (targetable == null)
		{
			return false;
		}

		if (_activeTargets.Contains(targetable))
		{
			Debug.LogWarning($"Already targeting {targetable.name}");
			return false;
		}

		if (!IsValidTarget(targetable))
		{
			Debug.LogWarning($"Attempted to target invalid targetable: {targetable.name}");
			return false;
		}

		if (!targetable.Target(this))
		{
			Debug.LogWarning($"Targetable {targetable.name} refused targeting");
			return false;
		}

		_activeTargets.Add(targetable);
		OnTargeted?.Invoke(targetable);
		return true;
	}

	/// <summary>
	/// Removes a target from active targets.
	/// </summary>
	public bool RemoveTarget(Targetable targetable)
	{
		if (targetable == null || !_activeTargets.Contains(targetable))
		{
			return false;
		}

		if (CurrentTarget == targetable)
		{
			CurrentTarget = null;
		}

		_activeTargets.Remove(targetable);
		targetable.UnTarget(this);
		OnUntargeted?.Invoke(targetable);
		return true;
	}

	/// <summary>
	/// Clears all active targets.
	/// </summary>
	public void ClearAllTargets()
	{
		var targets = new List<Targetable>(_activeTargets);
		foreach (Targetable target in targets)
		{
			RemoveTarget(target);
		}
		CurrentTarget = null;
	}

	/// <summary>
	/// Queries all valid targetables in the scene.
	/// </summary>
	public List<Targetable> GetAllTargets()
	{
		Targetable[] allTargetables = FindObjectsByType<Targetable>(FindObjectsSortMode.None);
		return allTargetables.Where(IsValidTarget).ToList();
	}

	/// <summary>
	/// Finds the nearest valid targetable to this controller.
	/// </summary>
	public Targetable GetNearestTarget()
	{
		List<Targetable> validTargets = GetAllTargets();
		return GetNearestTarget(validTargets);
	}

	/// <summary>
	/// Finds the nearest targetable from a provided list.
	/// </summary>
	public Targetable GetNearestTarget(List<Targetable> targets)
	{
		if (targets == null || targets.Count == 0)
		{
			return null;
		}

		Targetable nearest = null;
		float nearestDistance = float.MaxValue;

		foreach (Targetable target in targets)
		{
			if (target == null)
			{
				continue;
			}

			float distance = Vector3.Distance(transform.position, target.transform.position);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = target;
			}
		}

		return nearest;
	}

	/// <summary>
	/// Gets all valid targets within a specified radius.
	/// </summary>
	public List<Targetable> GetTargetsInRadius(float radius)
	{
		List<Targetable> allTargets = GetAllTargets();
		return allTargets.Where(t => Vector3.Distance(transform.position, t.transform.position) <= radius).ToList();
	}

	/// <summary>
	/// Checks if a targetable meets the criteria for this controller.
	/// </summary>
	private bool IsValidTarget(Targetable targetable)
	{
		if (targetable == null)
		{
			return false;
		}
		if (!targetable.IsTargetable)
		{
			return false;
		}
		if (_requireTangible && !targetable.IsTangible)
		{
			return false;
		}
		if ((targetable.Team & _categoryFilter) != 0)
		{
			return false;
		}

		return true;
	}

	private void OnDisable()
	{
		ClearAllTargets();
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if (CurrentTarget != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position, CurrentTarget.transform.position);
			Gizmos.DrawWireSphere(CurrentTarget.transform.position, 0.3f);
		}

		foreach (Targetable target in _activeTargets)
		{
			if (target != null && target != CurrentTarget)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(transform.position, target.transform.position);
				Gizmos.DrawWireSphere(target.transform.position, 0.2f);
			}
		}
	}

#endif
}
