using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Monobehavior script used for things that can be noticed and bitten by a fish.
/// Right now, it's only the fishing hook.
/// </summary>
public class Biteable : MonoBehaviour
{
	[SerializeField]
	private bool _noticeable = true;

	[SerializeField]
	private bool _biteable = true;

	public bool IsNoticeable => _noticeable;
	public bool IsBiteable => _biteable;

	public event Action<FishController> OnNotice;
	public event Action<FishController> OnUnNotice;
	public event Action<FishController> OnBite;

	public HashSet<FishController> NoticedFish { get; private set; }
	public FishController ActiveFish { get; private set; }

	private void Awake()
	{
		NoticedFish = new();
		ActiveFish = null;
	}

	public void SetNoticeable(bool noticeable)
	{
		_noticeable = noticeable;

		if (_noticeable == false)
		{
			foreach (FishController noticedFish in NoticedFish)
			{
				UnNotice(noticedFish);
			}
		}
	}

	public void SetBiteable(bool biteable)
	{
		_biteable = biteable;
		ActiveFish = null;
	}

	public bool Notice(FishController fish)
	{
		if (!_noticeable)
		{
			Debug.LogError("Attempted to notice unnoticeable biteable");
			return false;
		}
		if (NoticedFish.Contains(fish))
		{
			Debug.LogError("Attempted to notice biteable twice");
			return false;
		}
		if (ActiveFish != null)
		{
			Debug.LogError("Attempted to notice biteable when fight is happening");
			return false;
		}

		NoticedFish.Add(fish);
		OnNotice?.Invoke(fish);
		return true;
	}

	public bool UnNotice(FishController fish)
	{
		if (!NoticedFish.Contains(fish))
		{
			Debug.LogError("Attempted to unnotice biteable when wasn't noticed");
			return false;
		}

		NoticedFish.Remove(fish);
		OnUnNotice?.Invoke(fish);
		return true;
	}

	public bool Bite(FishController fish)
	{
		if (!_biteable)
		{
			Debug.LogError("Attempted to bite unbiteable biteable");
			return false;
		}
		if (ActiveFish != null)
		{
			Debug.LogError("Attempted to bite biteable when already bitten");
			return false;
		}

		ActiveFish = fish;

		// all other fish go away
		foreach (FishController noticedFish in NoticedFish)
		{
			UnNotice(noticedFish);
		}

		OnBite?.Invoke(fish);
		return true;
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, Vector2.one * 0.2f);
	}

#endif
}
