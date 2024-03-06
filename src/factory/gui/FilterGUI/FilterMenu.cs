using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class FilterMenu : PanelContainer
{
	private Container FilterList => GetNode<Container>("%FilterList");
	private ActionBarItem _actionBarItem;
	
	public static void Display(ActionBarItem item)
	{
		var gui = GD.Load<PackedScene>("res://src/factory/gui/FilterGUI/filter_menu.tscn").Instantiate<FilterMenu>();
		gui.Init(item);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	private void Init(ActionBarItem item)
	{
		_actionBarItem = item;
		UpdateInventory();
	}

	public override void _Ready()
	{
		base._Ready();
		UpdateInventory();
	}

	private void UpdateInventory()
	{
		FilterList.GetChildren().ToList().ForEach(child => child.QueueFree());
		//TODO: replace this with the item data from items.json
		Globals.PlayerInventory.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(AddInventoryItem);
	}
	
	private void AddInventoryItem(KeyValuePair<string, int> item)
	{
		var itemBox = GD.Load<PackedScene>("res://src/factory/gui/FilterGUI/filter_item.tscn").Instantiate<FilterItem>();
		itemBox.Init(item.Key, _actionBarItem);
		FilterList.AddChild(itemBox);
	}
}
