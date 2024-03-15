using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.factory.gui;
using Necromation.gui;

public partial class CrafterGUI : DeferredUpdate
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/CrafterGUI/CrafterGUI.tscn");

	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
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
	private ICrafter _crafter;
	
	// Static Accessor
	public static void Display(Inventory to, ICrafter crafter)
	{
		var gui = Scene.Instantiate<CrafterGUI>();
		gui.Init(to, crafter);
		Globals.FactoryScene.Gui.Open(gui);
	}

	// Constructor workaround.
	private void Init(Inventory to, ICrafter crafter)
	{
		_to = to;
		_crafter = crafter;

		AddUpdateListeners(new List<Inventory> { _to, _crafter.GetInputInventory(), _crafter.GetOutputInventory() });

		// Furnaces use this to display the recipe selection gui and cannot have their recipe changed.
		if (_crafter.GetRecipe() != null) ItemSelectionItemBox.Init(to, _crafter);
		else ItemSelectionItemBox.Visible = false;
		
		if (_crafter is ProgressTracker.IProgress progress)
		{
			ProgressBar.Init(progress);
		}
		
		Title.Text = _crafter.ItemType;
	}

	protected override void Update()
	{
		InventoryItem.UpdateInventory(_to, new List<Inventory> { _crafter.GetInputInventory()  }, InventoryItemList);
		CrafterItemBox.UpdateInventory(_crafter.GetInputInventory(), new List<Inventory> { _to }, SourceInventoryItemList, _crafter.GetRecipe(), true);
		CrafterItemBox.UpdateInventory(_crafter.GetOutputInventory(), new List<Inventory> { _to }, OutputInventoryItemList, _crafter.GetRecipe(), false);
		Dirty = false;
	}
}
