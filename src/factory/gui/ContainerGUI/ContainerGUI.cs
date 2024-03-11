using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class ContainerGui : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/ContainerGUI/ContainerGui.tscn");

	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container InventoryList => GetNode<Container>("%InventoryList");
	private Container ContainerInventoryList => GetNode<Container>("%ContainerInventoryList");
	private Label Title => GetNode<Label>("%Title");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _to;
	private Inventory _from;
	
	// Static Accessor
	public static void Display(Inventory to, Inventory from, string title)
	{
		var gui = Scene.Instantiate<ContainerGui>();
		gui.Init(to, from, title);
		Globals.FactoryScene.Gui.Open(gui);
	}
	
	// Constructor workaround.
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
		InventoryItem.UpdateInventory(_to, new List<Inventory> { _from }, InventoryList);
	}
	
	private void UpdateSourceInventory()
	{
		InventoryItem.UpdateInventory(_from, new List<Inventory> { _to }, ContainerInventoryList);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_to?.Listeners.Remove(UpdatePlayerInventory);
		_from?.Listeners.Remove(UpdateSourceInventory);
	}
}
