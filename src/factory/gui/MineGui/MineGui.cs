using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class MineGui : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/MineGui/MineGui.tscn");

	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container InventoryList => GetNode<Container>("%InventoryList");
	private Container ContainerInventoryList => GetNode<Container>("%ContainerInventoryList");
	private ProgressTracker ProgressBar => GetNode<ProgressTracker>("%ProgressBar");
	private Label Title => GetNode<Label>("%Title");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _to;
	private Inventory _from;

	// Static Accessor
	public static void Display(Inventory to, Inventory from, ProgressTracker.IProgress progress, string title)
	{
		var gui = Scene.Instantiate<MineGui>();
		gui.Init(to, from, progress, title);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}
	
	// Constructor workaround.
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
		InventoryItem.UpdateInventory(_to, null, InventoryList);
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
