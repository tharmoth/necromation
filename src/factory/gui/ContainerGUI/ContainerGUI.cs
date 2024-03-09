using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class ContainerGui : PanelContainer
{
	private static readonly PackedScene _scene = GD.Load<PackedScene>("res://src/factory/gui/ContainerGUI/ContainerGui.tscn");
	private static readonly PackedScene _inventoryItemScene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_item_box.tscn");
	public static void Display(Inventory to, Inventory from, string title)
	{
		var gui = _scene.Instantiate<ContainerGui>();
		gui.Init(to, from, title);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}
	
	private Inventory _to;
	private Inventory _from;
	private Container InventoryList => GetNode<Container>("%InventoryList");
	private Container ContainerInventoryList => GetNode<Container>("%ContainerInventoryList");
	private Label Title => GetNode<Label>("%Title");
	
	private void Init(Inventory to, Inventory from, string title)
	{
		_to = to;
		_from = from;
		
		_to.Listeners.Add(UpdatePlayerInventory);
		UpdatePlayerInventory();
		
		from.Listeners.Add(UpdateSourceInventory);
		UpdateSourceInventory();

		Title.Text = title;
	}

	private void UpdatePlayerInventory()
	{
		UpdateInventory(_to, _from, InventoryList);
	}
	
	private void UpdateSourceInventory()
	{
		UpdateInventory(_from, _to, ContainerInventoryList);
	}

	private void UpdateInventory(Inventory from, Inventory to, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());
		from.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(item => AddInventoryItem(item, from, to, list));
	}
	
	private void AddInventoryItem(KeyValuePair<string, int> item, Inventory from, Inventory to, Container list)
	{
		var inventoryItem = _inventoryItemScene.Instantiate<InventoryItem>();
		inventoryItem.Init(from, to, item.Key, item.Value);
		list.AddChild(inventoryItem);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_to?.Listeners.Remove(UpdatePlayerInventory);
		_from?.Listeners.Remove(UpdateSourceInventory);
	}
}
