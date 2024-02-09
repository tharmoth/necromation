using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public partial class Inventory : Node
{
	private static Inventory _instance;
	public static Inventory Instance => _instance;
	
	public readonly List<Action> Listeners = new();

	private readonly Dictionary<string, int> _items = new();

	public override void _EnterTree(){
		if(_instance != null){
			QueueFree(); // The Singleton is already loaded, kill this instance
		}
		_instance = this;
		_instance.AddItem("Stone", 10);
	}

	public ImmutableDictionary<string, int> Items => _items.ToImmutableDictionary();
	
	public void AddItem(string item, int count = 1)
	{
		_items.TryGetValue(item, out var currentCount);
		_items[item] = currentCount + count;
		Listeners.ForEach(listener => listener());
	}
	
	public bool RemoveItem(string item, int count = 1)
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
		if (!from.RemoveItem(item, count)) return false;
		to.AddItem(item, count);
		return true;
	}
	
}
