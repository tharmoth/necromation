using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.factory.gui;

public partial class MineGui : DeferredUpdate
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
		Globals.FactoryScene.Gui.Open(gui);
	}
	
	// Constructor workaround.
	private void Init(Inventory to, Inventory from, ProgressTracker.IProgress progress, string title)
	{
		_to = to;
		_from = from;
		
		AddUpdateListeners(new List<Inventory> { _to, _from });

		Title.Text = title;
		ProgressBar.Init(progress.GetProgressPercent);
		
		Update();
	}
	
	protected override void Update()
	{
		InventoryItemBox.UpdateInventory(_to, null, InventoryList);
		InventoryItemBox.UpdateInventory(_from, new List<Inventory> { _to }, ContainerInventoryList);
		Dirty = false;
	}
}
