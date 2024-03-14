using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.factory.gui;
using Necromation.gui;

public partial class InventoryGUI : DeferredUpdate
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
		Globals.FactoryScene.Gui.Open(gui);
	}

	// Constructor workaround.
	private void Init(Inventory inventory)
	{
		_inventory = inventory;
		
		Globals.ResearchListeners.Add(FlagDirty);
		AddUpdateListeners(new List<Inventory> { _inventory });
	}

	protected override void Update()
	{
		InventoryItem.UpdateInventory(_inventory, null, InventoryItemList);
		InventoryRecipeBox.UpdateRecipes(_inventory, RecipeButtonList);	
		Dirty = false;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		// Godot Signals are automatically disconnected when the node is freed
		// But since we are using C# events, we need to manually disconnect them
		Globals.ResearchListeners.Remove(FlagDirty);
	}
}
