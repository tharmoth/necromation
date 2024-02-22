using Godot;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class ItemList : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Globals.PlayerInventory.Listeners.Add(Update);
		Update();
	}

	private void Update()
	{
		RemoveMissingItems();
		AddMissingItems();
		GetChildren().OfType<ItemLabel>().ToList().ForEach(child => child.Update());
	}
	
	private void RemoveMissingItems()
	{
		foreach (var label in GetChildren().OfType<ItemLabel>().ToHashSet())
		{
			if (Globals.PlayerInventory.Items.ContainsKey(label.ItemType)) continue;
			
			label.QueueFree();
		}
	}

	private void AddMissingItems()
	{
		foreach (var item in Globals.PlayerInventory.Items.Keys)
		{
			if (GetChildren().OfType<ItemLabel>().Any(label => label.ItemType == item)) continue;
			
			var itemLabel = GD.Load<PackedScene>("res://src/gui/item_label.tscn").Instantiate<ItemLabel>();
			itemLabel.ItemType = item;
			itemLabel.Inventory = Globals.PlayerInventory;
			AddChild(itemLabel);
		}
	}
}
