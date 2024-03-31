using Godot;
using System;
using System.Collections.Generic;
using Necromation;
using Necromation.factory.gui;
using Necromation.gui;

public partial class BarracksGui : DeferredUpdate
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/BarracksGui/BarracksGui.tscn");

	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private Container SourceInventoryItemList => GetNode<Container>("%SourceInventoryItemList");
	private Container OutputInventoryItemList => GetNode<Container>("%OutputInventoryItemList");
	private ItemSelectionItemBox ItemSelectionItemBox => GetNode<ItemSelectionItemBox>("%ItemSelectionItemBox");
	private ProgressTracker ProgressBar => GetNode<ProgressTracker>("%ProgressBar");
	private Label Title => GetNode<Label>("%Title");
	private Button OrdersButton => GetNode<Button>("%OrdersButton");
	private Button PositionButton => GetNode<Button>("%PositionButton");
	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _to;
	private Barracks _barracks;
	
	// Static Accessor
	public static void Display(Inventory to, Barracks barracks)
	{
		var gui = Scene.Instantiate<BarracksGui>();
		gui.Init(to, barracks);
		Globals.FactoryScene.Gui.Open(gui);
	}

	// Constructor workaround.
	private void Init(Inventory to, Barracks barracks)
	{
		_to = to;
		_barracks = barracks;

		AddUpdateListeners(new List<Inventory> { _to, _barracks.Inventory});

		ProgressBar.Init(barracks);
		
		Title.Text = barracks.ItemType + " - " + barracks.Commander.CommanderName;
		
		PositionButton.Pressed += () => BarracksBoxDraw.Display(barracks.Commander);
		OrdersButton.Pressed += () => CommandsGui.Display(barracks.Commander);
	}

	protected override void Update()
	{
		InventoryItemBox.UpdateInventory(_to, new List<Inventory> { _barracks.Inventory }, InventoryItemList);
		// InventoryItem.UpdateInventory(_barracks.GetInputInventory(), new List<Inventory> { _to }, SourceInventoryItemList);
		InventoryItemBox.UpdateInventory(_barracks.Inventory, new List<Inventory> { _to }, OutputInventoryItemList);
		UnitCountLabel.Text = _barracks.Inventory.CountItems().ToString() + " / " + _barracks.Commander.CommandCap;
		Dirty = false;
	}
}
