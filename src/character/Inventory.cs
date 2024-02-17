using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public partial class Inventory : Node
{
	public readonly List<Action> Listeners = new();

	private readonly Dictionary<string, int> _items = new();

	public ImmutableDictionary<string, int> Items => _items.ToImmutableDictionary();
	
	public void TransferAllTo(Inventory other)
	{
		other.Insert(_items);
		_items.Clear();
		Listeners.ForEach(listener => listener());
	}
	
	public void Insert(Dictionary<string, int> items)
	{
		items.ToList().ForEach(entry => Insert(entry.Key, entry.Value));
	}
	
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
	
	public int CountItem(string item)
	{
		return _items.TryGetValue(item, out var count) ? count : 0;
	}

	public static bool TransferItem(Inventory from, Inventory to, string item, int count = 1)
	{
		if (from.CountItem(item) < count) return false;
		from.Remove(item, count);
		to.Insert(item, count);
		return true;
	}

	public int CountAllItems()
	{
		return _items.Values.Sum();
	}

	public string GetFirstItem()
	{
		return _items.Keys.FirstOrDefault();
	}
	
	public virtual bool CanAcceptItem(string item)
	{
		return true;
	}

}
