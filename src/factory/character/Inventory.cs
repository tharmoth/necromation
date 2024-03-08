using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Necromation;

public partial class Inventory : ITransferTarget
{
	public readonly List<Action> Listeners = new();
	private readonly Dictionary<string, int> _items = new();
	public ImmutableDictionary<string, int> Items => _items.ToImmutableDictionary();
	
	int itemCount = 0;

	public int CountItem(string item) => _items.TryGetValue(item, out var count) ? count : 0;
	public int CountAllItems() => itemCount;
	
	public void Clear()
	{
		foreach (var item in _items.Keys.ToList())
		{
			Remove(item, _items[item]);
		}
	}

	/**************************************************************************
	 * Utility Methods                                                        *
	 **************************************************************************/
	public static bool TransferItem(ITransferTarget from, ITransferTarget to, string item, int count = 1)
	{
		if (!to.CanAcceptItems(item, count)) return false;
		from.Remove(item, count);
		to.Insert(item, count);
		return true;
	}
	
	public static void TransferAllTo(Inventory from, Inventory to)
	{
		foreach (var (item, count) in from.Items)
		{
			TransferItem(from, to, item, count);
		}
	}

	#region ITransferTarget Implementation
	/**************************************************************************
	 * ITransferTarget Methods                                                *
	 **************************************************************************/
	public void Insert(string item, int count = 1)
	{
		_items.TryGetValue(item, out var currentCount);
		_items[item] = currentCount + count;
		itemCount += count;
		Listeners.ForEach(listener => listener());
	}
	public bool Remove(string item, int count = 1)
	{
		if (!_items.TryGetValue(item, out var currentCount) || currentCount < count) return false;
		currentCount -= count;
		itemCount -= count;
		if (currentCount == 0)
		{
			_items.Remove(item);
		}
		else
		{
			_items[item] = currentCount;
		}
		Listeners.ForEach(listener => listener());
		return true;
	}
	public virtual bool CanAcceptItems(string item, int count = 1) => true;
	public string GetFirstItem() => _items.Keys.FirstOrDefault();
	public List<string> GetItems() => _items.Keys.ToList();
	public List<Inventory> GetInventories() => new() { this };
	#endregion

	/**************************************************************************
	 * Save/Load Methods                                                      *
	 **************************************************************************/
	public void Load(Godot.Collections.Dictionary dictionary)
	{
		_items.Clear();
		foreach (var entry in dictionary)
		{
			// This shouldn't happen unless the file is an old version or manually edited.
			if (!CanAcceptItems(entry.Key.ToString(), (int)entry.Value)) continue;
			_items.Add(entry.Key.ToString(), (int)entry.Value);
			itemCount += (int)entry.Value;
		}
		Listeners.ForEach(listener => listener());
	}
	
	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		return new Godot.Collections.Dictionary<string, Variant>(Items.ToDictionary(pair => pair.Key, pair => (Variant)pair.Value));
	}
}
