using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// CURRENTLY UNUSED.
// will be useful for the minigame down the line, when there are possibly multiple things for a skill or attack to target.

/// <summary>
/// Placed on anything that can be targeted by attacks during combat.
/// <c>TargetingController</c> instances can then scan for this component and potentially target it.
/// </summary>
public class Targetable : MonoBehaviour
{
	[Header("References (optional)")]
	[SerializeField]
	private Damageable _damageable;

	// unsure how this will be used.
	[SerializeField]
	private Collider2D _targetingHitbox;

	[Header("Parameters")]
	[Tooltip("Whether or not this gameobject can be targeted by TargetingController")]
	[SerializeField]
	private bool _isTargetable = true;

	[Tooltip("Whether or not this gameobject should be targetable by 'physical' actions (ex. a grapple skill)")]
	[SerializeField]
	private bool _isTangible = true;

	[Tooltip("What team this target belongs to")]
	[SerializeField]
	private CombatTeam _team = CombatTeam.None;

	//

	public bool IsTargetable => _isTargetable;
	public bool IsTangible => _isTangible;
	public CombatTeam Team => _team;
	public bool HasDamageable => _damageable != null;
	public Damageable Damageable => _damageable;

	// Helper properties for quick health checks
	public bool HasHealth => _damageable != null && _damageable.HasHealth;
	public HealthController HealthController => _damageable != null ? _damageable.Health : null;

	public event Action<TargetingController> OnTargeted;
	public event Action<TargetingController> OnUntargeted;

	public IReadOnlyCollection<TargetingController> Targeters => _targeters;

	//

	private HashSet<TargetingController> _targeters = new();

	//

	public void OnEnable()
	{
		_targeters = new();
	}

	public void SetTargetable(bool targetable)
	{
		_isTargetable = targetable;

		if (!_isTargetable)
		{
			// Untarget all targeters when becoming untargetable
			var targetersToRemove = new List<TargetingController>(_targeters);
			foreach (TargetingController targeter in targetersToRemove)
			{
				UnTarget(targeter);
			}
		}
	}

	public void SetTangible(bool tangible)
	{
		_isTangible = tangible;

		if (!_isTangible)
		{
			// Untarget all targeters that require tangibility
			var targetersToRemove = new List<TargetingController>(_targeters.Where(t => t.RequireTangible));
			foreach (TargetingController targeter in targetersToRemove)
			{
				UnTarget(targeter);
			}
		}
	}

	public bool Target(TargetingController targeter)
	{
		if (!_isTargetable)
		{
			Debug.LogWarning($"Attempted to target untargetable object: {gameObject.name}");
			return false;
		}

		if (_targeters.Contains(targeter))
		{
			Debug.LogWarning($"Targeter {targeter.gameObject.name} is already targeting {gameObject.name}");
			return false;
		}

		_targeters.Add(targeter);
		OnTargeted?.Invoke(targeter);

		return true;
	}

	public bool UnTarget(TargetingController targeter)
	{
		if (!_targeters.Contains(targeter))
		{
			Debug.LogWarning(
				$"Attempted to untarget {gameObject.name} when it wasn't being targeted by {targeter.gameObject.name}"
			);
			return false;
		}

		_targeters.Remove(targeter);
		OnUntargeted?.Invoke(targeter);

		return true;
	}
}
