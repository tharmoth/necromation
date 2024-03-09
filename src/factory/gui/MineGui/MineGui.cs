using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class MineGui : PanelContainer
{
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/MineGui/MineGui.tscn");
	private static readonly PackedScene InventoryItemScene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_item_box.tscn");
	
	public static void Display(Inventory to, Inventory from, ProgressTracker.IProgress progress, string title)
	{
		var gui = Scene.Instantiate<MineGui>();
		gui.Init(to, from, progress, title);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}
	
	private Inventory _to;
	private Inventory _from;
	private Container InventoryList => GetNode<Container>("%InventoryList");
	private Container ContainerInventoryList => GetNode<Container>("%ContainerInventoryList");
	private ProgressTracker ProgressBar => GetNode<ProgressTracker>("%ProgressBar");
	private Label Title => GetNode<Label>("%Title");
	
	private void Init(Inventory to, Inventory from, ProgressTracker.IProgress progress, string title)
	{
		_to = to;
		_from = from;
		
		_to.Listeners.Add(UpdatePlayerInventory);
		UpdatePlayerInventory();
		
		from.Listeners.Add(UpdateSourceInventory);
		UpdateSourceInventory();

		Title.Text = title;
		ProgressBar.Init(progress);
	}

	private void UpdatePlayerInventory()
	{
		UpdateInventory(_to, null, InventoryList);
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
		var inventoryItem = InventoryItemScene.Instantiate<InventoryItem>();
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
