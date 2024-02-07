using Godot;
using System;
using System.Linq;
using Necromation.gui;

public partial class ItemList : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Inventory.Instance.Listeners.Add(Update);
		Update();
	}

	private void Update()
	{
		RemoveMissingItems();
		AddMissingItems();

		foreach (var child in GetChildren())
		{
			if (child is ItemLabel label)
			{
				label.Update();
			}
		}
	}
	
	private void RemoveMissingItems()
	{
		foreach (var label in GetChildren().OfType<ItemLabel>().ToHashSet())
		{
			if (Inventory.Instance.Items.ContainsKey(label.ItemType)) continue;
			
			label.QueueFree();
		}
	}

	private void AddMissingItems()
	{
		foreach (var item in Inventory.Instance.Items.Keys)
		{
			if (GetChildren().OfType<ItemLabel>().Any(label => label.ItemType == item)) continue;
			
			var itemLabel = GD.Load<PackedScene>("res://src/gui/item_label.tscn").Instantiate<ItemLabel>();
			itemLabel.ItemType = item;
			AddChild(itemLabel);
		}
	}
}
