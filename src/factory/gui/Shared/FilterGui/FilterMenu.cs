using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class FilterMenu : PanelContainer
{
	private Container FilterList => GetNode<Container>("%FilterList");
	private HotBarItemBox _hotBarItemBox;
	
	public static void Display(HotBarItemBox itemBox)
	{
		var gui = GD.Load<PackedScene>("res://src/factory/gui/Shared/FilterGui/FilterMenu.tscn").Instantiate<FilterMenu>();
		gui.Init(itemBox);
		Globals.FactoryScene.Gui.Open(gui);
	}

	private void Init(HotBarItemBox itemBox)
	{
		_hotBarItemBox = itemBox;
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
		var itemBox = GD.Load<PackedScene>("res://src/factory/gui/Shared/FilterGui/FilterItemBox.tscn").Instantiate<FilterItem>();
		itemBox.Init(item.Key, _hotBarItemBox);
		FilterList.AddChild(itemBox);
	}
}
