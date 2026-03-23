using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryState
{
	// Update UI

	public event Action OnGoldChanged;
	public event Action OnBoatChanged;
	public event Action OnFishInventoryChanged;
	public event Action OnItemInventoryChanged;
	public event Action OnRodInventoryChanged;
	public event Action OnRodAttachmentInventoryChanged;
	public event Action OnRodLoadoutChanged;

	public int CurrentFishCapacity { get; private set; } = 20;
	public int Gold { get; private set; } = 0;

	// ensures that only this class can modify contents of the lists
	private readonly List<FishInstance> _fishes = new();
	private readonly List<RodSO> _rods = new();
	private readonly List<RodAttachmentSO> _rodAttachments = new();
	public IReadOnlyList<FishInstance> Fishes => _fishes;
	public IReadOnlyList<RodSO> Rods => _rods;
	public IReadOnlyList<RodAttachmentSO> RodAttachments => _rodAttachments;

	public RodSO EquippedRod { get; private set; }

	public int MaxBobberSlots { get; private set; } = 2;
	public int MaxTrinketSlots { get; private set; } = 3;

	private readonly List<RodAttachmentSO> _equippedBobbers = new();
	private readonly List<RodAttachmentSO> _equippedTrinkets = new();
	public IReadOnlyList<RodAttachmentSO> EquippedRodBobbers => _equippedBobbers;
	public IReadOnlyList<RodAttachmentSO> EquippedRodTrinkets => _equippedTrinkets;

	// todo: track which items are newly discovered, so we can mark them in the inventory
	// ... architecture to be discussed

	// gold

	public void AddGold(int amount)
	{
		if (Gold + amount < 0)
		{
			Debug.LogError("attempted to ChangeGold() to a negative gold amount");
			return;
		}

		Gold += amount;
		OnGoldChanged?.Invoke();
	}

	public void SubtractGold(int amount)
	{
		if (Gold - amount < 0)
		{
			Debug.LogError("attempted to ChangeGold() to a negative gold amount");
			return;
		}

		Gold -= amount;
		OnGoldChanged?.Invoke();
	}

	public void SetGold(int amount)
	{
		if (amount < 0)
		{
			Debug.LogError("attempted to ChangeGold() to a negative gold amount");
			return;
		}

		Gold = amount;
		OnGoldChanged?.Invoke();
	}

	// fish

	public void AddFish(FishInstance fish)
	{
		_fishes.Add(fish);
		OnFishInventoryChanged?.Invoke();
	}

	public void RemoveFish(int idx)
	{
		_fishes.RemoveAt(idx);
		OnFishInventoryChanged?.Invoke();
	}

	public void RemoveFish(FishInstance fish)
	{
		if (fish != null)
		{
			_fishes.Remove(fish);
			OnFishInventoryChanged?.Invoke();
		}
	}

	public void RemoveFish(string fishId, int count = 1)
	{
		int removedCount = 0;
		for (int i = _fishes.Count - 1; i >= 0; i--)
		{
			if (_fishes[i].Id == fishId)
			{
				_fishes.RemoveAt(i);
				removedCount++;
				if (removedCount >= count)
				{
					break;
				}
			}
		}
		OnFishInventoryChanged?.Invoke();
	}

	public bool HasFish(string fishId)
	{
		bool hasFish = false;

		foreach (FishInstance fish in _fishes)
		{
			if (fish.Id == fishId)
			{
				hasFish = true;
			}
		}

		return hasFish;
	}

	public bool HasFish(string fishId, out int amount)
	{
		bool hasFish = false;
		amount = 0;

		foreach (FishInstance fish in _fishes)
		{
			if (fish.Id == fishId)
			{
				amount++;
				hasFish = true;
			}
		}

		return hasFish;
	}

	//public void Replace()
	//{
	/*
	This function is called whenever a fish is caught and backpack is full.
	Show replaceUI and choose what to do with that fish.
	Not used in current version.
	*/
	//}

	public void PickingFish(FishInstance fish)
	{
		/*
		This function is called whenever a fish is caught, to check whether what to do.
		If backpack is full, then call Replace function to show replaceUI and choose what to do with that fish.
		Otherwise, just add it.

		Not used in current version.
		*/
		//if()
		//{
		//AddFish(fish);
		//}
		//else
		//{
		//Replace
		//}
	}

	public void SellFishByIndex(int idx, bool isForGold = true)
	{
		// Add money to player inventory here later
		if (isForGold)
		{
			Gold += Mathf.FloorToInt(_fishes[idx].SellValue);
		}
		else
		{
			//is for essence
			return;
		}

		_fishes.RemoveAt(idx);
		OnFishInventoryChanged?.Invoke();
		OnGoldChanged?.Invoke();
		// space for later weight or capacity update
	}

	public void SellFish(FishInstance fish, bool isForGold = true)
	{
		// Add money to player inventory here later
		if (isForGold)
		{
			Gold += Mathf.FloorToInt(fish.SellValue);
			// CurrentWeight -= fish.Weight;
		}
		else
		{
			//is for essence
			return;
		}

		_fishes.Remove(fish);
		OnFishInventoryChanged?.Invoke(); // update UI when fish inventory changes
		OnGoldChanged?.Invoke();
		// space for later weight or capacity update
		// items
	}

	// rods

	public void AddRod(RodSO rod, bool equip = true)
	{
		if (_rods.Contains(rod))
		{
			Debug.LogError("Attempted to add an already-owned rod");
			return;
		}

		_rods.Add(rod);
		OnRodInventoryChanged?.Invoke();

		if (equip)
		{
			EquipRod(rod);
		}
	}

	// note: only for testing purposes. these are not throwable in game!
	public void ThrowRod(int idx)
	{
		if (_rods.Count == 1)
		{
			Debug.LogError("Attempted to remove the only owned rod");
			return;
		}

		RodSO removedRod = _rods[idx];
		_rods.RemoveAt(idx);

		if (removedRod == EquippedRod)
		{
			EquipRod(_rods[0]);
		}

		OnRodInventoryChanged?.Invoke();
	}

	public void BuyRod(RodSO rod, int price)
	{
		if (Gold >= price)
		{
			AddGold(-price);
			AddRod(rod);
		}
		else
		{
			// Not enough gold
			return;
			// Show some UI later
		}
	}

	// rod attachments

	public void AddRodAttachment(RodAttachmentSO rodAttachment, bool equip = true)
	{
		if (_rodAttachments.Contains(rodAttachment))
		{
			Debug.LogError("Attempted to add an already-owned rod attachment");
			return;
		}

		_rodAttachments.Add(rodAttachment);
		OnRodAttachmentInventoryChanged?.Invoke();

		if (equip)
		{
			if (rodAttachment.AttachmentType == RodAttachmentType.Bobber)
			{
				EquipRodBobber(rodAttachment);
			}
			else
			{
				EquipRodTrinket(rodAttachment);
			}
		}
	}

	// note: only for testing purposes. these are not throwable in game!
	public void ThrowRodAttachment(int idx)
	{
		RodAttachmentSO removed = _rodAttachments[idx];
		_rodAttachments.RemoveAt(idx);

		_equippedBobbers.RemoveAll(b => b == removed);
		_equippedTrinkets.RemoveAll(t => t == removed);

		OnRodAttachmentInventoryChanged?.Invoke();
		OnRodLoadoutChanged?.Invoke();
	}

	public void BuyRodAttachment(RodAttachmentSO rodAttachment, int price)
	{
		if (Gold >= price)
		{
			AddGold(-price);
			AddRodAttachment(rodAttachment);
		}
		else
		{
			// Not enough gold
			return;
			// Show some UI later
		}
	}

	public void EquipRod(RodSO rod)
	{
		EquippedRod = rod;
		OnRodLoadoutChanged?.Invoke();
	}

	public void EquipRodBobber(RodAttachmentSO attachment, int slot = 0)
	{
		if (attachment != null && attachment.AttachmentType != RodAttachmentType.Bobber)
		{
			Debug.LogError("Attempted to equip a non-bobber as a bobber");
			return;
		}
		if (slot < 0 || slot >= MaxBobberSlots)
		{
			Debug.LogError($"Bobber slot {slot} is not unlocked");
			return;
		}

		// grow list to fit if needed
		while (_equippedBobbers.Count <= slot)
		{
			_equippedBobbers.Add(null);
		}

		_equippedBobbers[slot] = attachment;
		OnRodLoadoutChanged?.Invoke();
	}

	public void EquipRodTrinket(RodAttachmentSO attachment, int slot = 0)
	{
		if (attachment != null && attachment.AttachmentType != RodAttachmentType.Trinket)
		{
			Debug.LogError("Attempted to equip a non-trinket as a trinket");
			return;
		}
		if (slot < 0 || slot >= MaxTrinketSlots)
		{
			Debug.LogError($"Trinket slot {slot} is not unlocked");
			return;
		}

		while (_equippedTrinkets.Count <= slot)
		{
			_equippedTrinkets.Add(null);
		}

		_equippedTrinkets[slot] = attachment;
		OnRodLoadoutChanged?.Invoke();
	}

	public void UnlockBobberSlot()
	{
		if (MaxBobberSlots >= 2)
		{
			Debug.LogWarning("All bobber slots already unlocked");
			return;
		}
		MaxBobberSlots++;
		OnRodLoadoutChanged?.Invoke();
	}

	public void UnlockTrinketSlot()
	{
		if (MaxTrinketSlots >= 3)
		{
			Debug.LogWarning("All trinket slots already unlocked");
			return;
		}
		MaxTrinketSlots++;
		OnRodLoadoutChanged?.Invoke();
	}

	public void Save()
	{
		// unimplemented
	}

	public void Load()
	{
		// unimplemented
	}
}
