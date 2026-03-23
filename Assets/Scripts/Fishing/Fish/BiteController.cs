using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiteController : MonoBehaviour
{
	[Header("References (optional)")]
	[Tooltip("Visual point at which the fish bites the hook")]
	[SerializeField]
	private Transform _bitePoint;

	[SerializeField]
	private BiteDetectionZone _detectionArea;

	[SerializeField]
	private BiteDetectionZone _biteArea;

	public bool ReadyToBite { get; private set; }

	public Transform BitePoint => _bitePoint;

	// invoked the moment a biteable overlaps _biteArea
	public event Action<Biteable> OnCanBite;

	// tracks all biteables, noticeable or not.
	// non-noticeable ones are filtered out later.
	private readonly List<Biteable> _biteablesInDetectionZone = new();

	/** Unity Messages **/

	private void Awake()
	{
		_biteablesInDetectionZone.Clear();
	}

	private void OnEnable()
	{
		if (_detectionArea != null)
		{
			_detectionArea.OnBiteableEntered += OnDetectionZoneEntered;
			_detectionArea.OnBiteableExited += OnDetectionZoneExited;
		}
		if (_biteArea != null)
		{
			_biteArea.OnBiteableEntered += OnBiteZoneEntered;
		}
	}

	private void OnDisable()
	{
		if (_detectionArea != null)
		{
			_detectionArea.OnBiteableEntered -= OnDetectionZoneEntered;
			_detectionArea.OnBiteableExited -= OnDetectionZoneExited;
		}
		if (_biteArea != null)
		{
			_biteArea.OnBiteableEntered -= OnBiteZoneEntered;
		}
	}

	/** Event Handlers **/

	private void OnDetectionZoneEntered(Biteable biteable)
	{
		_biteablesInDetectionZone.Add(biteable);
	}

	private void OnDetectionZoneExited(Biteable biteable)
	{
		_biteablesInDetectionZone.Remove(biteable);
	}

	private void OnBiteZoneEntered(Biteable biteable)
	{
		if (biteable.IsBiteable && ReadyToBite)
		{
			OnCanBite?.Invoke(biteable);
		}
	}

	/** Public Methods **/

	public void SetReadyToBite(bool val)
	{
		ReadyToBite = val;
	}

	public List<Biteable> CheckBiteables()
	{
		// Clean up any destroyed objects
		_biteablesInDetectionZone.RemoveAll(b => b == null);

		return new List<Biteable>(_biteablesInDetectionZone.Where(b => b.IsNoticeable));
	}
}
