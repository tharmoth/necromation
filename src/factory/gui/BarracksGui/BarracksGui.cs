using Godot;
using System;
using Necromation;
using Necromation.gui;

public partial class BarracksGui : PanelContainer
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
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	// Constructor workaround.
	private void Init(Inventory to, Barracks barracks)
	{
		_to = to;
		_barracks = barracks;
		
		_to.Listeners.Add(UpdatePlayerInventory);
		UpdatePlayerInventory();
		
		_barracks.GetInputInventory().Listeners.Add(UpdateSourceInventory);
		UpdateSourceInventory();
		
		_barracks.GetOutputInventory().Listeners.Add(UpdateOutputInventory);
		UpdateOutputInventory();

		ItemSelectionItemBox.Init(to, _barracks);
		
		ProgressBar.Init(barracks);
		
		Title.Text = barracks.ItemType;
	}

	private void UpdatePlayerInventory()
	{
		InventoryItem.UpdateInventory(_to, _barracks.GetInputInventory(), InventoryItemList);
	}
	
	private void UpdateSourceInventory()
	{
		InventoryItem.UpdateInventory(_barracks.GetInputInventory(), _to, SourceInventoryItemList);
	}
	
	private void UpdateOutputInventory()
	{
		InventoryItem.UpdateInventory(_barracks.GetOutputInventory(), _to, OutputInventoryItemList);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_to?.Listeners.Remove(UpdatePlayerInventory);
		_barracks?.GetInputInventory().Listeners.Remove(UpdateSourceInventory);
		_barracks?.GetOutputInventory().Listeners.Remove(UpdateOutputInventory);
	}
}
