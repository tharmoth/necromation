using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Necromation;

public partial class Inventory : Node, ITransferTarget
{
	public readonly List<Action> Listeners = new();
	private readonly Dictionary<string, int> _items = new();
	public ImmutableDictionary<string, int> Items => _items.ToImmutableDictionary();

	public int CountItem(string item) => _items.TryGetValue(item, out var count) ? count : 0;
	public int CountAllItems() => _items.Values.Sum();
	
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
		Listeners.ForEach(listener => listener());
	}
	public bool Remove(string item, int count = 1)
	{
		if (!_items.TryGetValue(item, out var currentCount) || currentCount < count) return false;
		_items[item] -= count;
		if (_items[item] == 0) _items.Remove(item);
		Listeners.ForEach(listener => listener());
		return true;
	}
	public bool CanAcceptItems(string item, int count = 1) => true;
	public string GetFirstItem() => _items.Keys.FirstOrDefault();
	public List<string> GetItems() => _items.Keys.ToList();
	public List<Inventory> GetInventories() => new() { this };
	#endregion
}
