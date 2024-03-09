using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class InventoryGUI : Control
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_gui.tscn");
	
	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private Container RecipeButtonList => GetNode<Container>("%RecipeButtonList");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _inventory;

	// Static Accessor
	public static void Display(Inventory inventory)
	{
		var gui = Scene.Instantiate<InventoryGUI>();
		gui.Init(inventory);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	// Constructor workaround.
	private void Init(Inventory inventory)
	{
		_inventory = inventory;
		
		_inventory.Listeners.Add(UpdatePlayerInventory);
		UpdatePlayerInventory();
		
		Globals.ResearchListeners.Add(UpdateRecipes);
		UpdateRecipes();
	}
	
	private void UpdatePlayerInventory()
	{
		InventoryItem.UpdateInventory(_inventory, null, InventoryItemList);
	}
	
	private void UpdateRecipes()
	{
		InventoryRecipeBox.UpdateRecipes(_inventory, RecipeButtonList);
	}
	
	
	public override void _ExitTree()
	{
		base._ExitTree();
		// Godot Signals are automatically disconnected when the node is freed
		// But since we are using C# events, we need to manually disconnect them
		Globals.ResearchListeners.Remove(UpdateRecipes);
		_inventory.Listeners.Remove(UpdatePlayerInventory);
	}
}
