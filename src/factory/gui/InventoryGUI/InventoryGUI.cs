using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class InventoryGUI : Control
{
	
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private Container RecipeButtonList => GetNode<Container>("%RecipeButtonList");
	private Inventory _inventory;

	public static InventoryGUI Display(Inventory inventory)
	{
		var gui = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_gui.tscn").Instantiate<InventoryGUI>();
		gui._inventory = inventory;
		Globals.FactoryScene.Gui.AddChild(gui);
		return gui;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		// Godot Signals are automatically disconnected when the node is freed
		// But since we are using C# events, we need to manually disconnect them
		Globals.ResearchListeners.Remove(UpdateRecipes);
		_inventory.Listeners.Remove(UpdateInventory);
	}

	public override void _Ready()
	{
		base._Ready();
		
		Globals.ResearchListeners.Add(UpdateRecipes);
		UpdateRecipes();
		
		_inventory.Listeners.Add(UpdateInventory);
		UpdateInventory();
	}
	
	private void UpdateInventory()
	{
		InventoryItemList.GetChildren().ToList().ForEach(child => child.QueueFree());
		_inventory.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(AddInventoryItem);
	}

	private void AddInventoryItem(KeyValuePair<string, int> item)
	{
		var inventoryItem = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_item_box.tscn").Instantiate<InventoryItem>();
		inventoryItem.Init(_inventory, null, item.Key, item.Value);
		InventoryItemList.AddChild(inventoryItem);
	}
	
	private void UpdateRecipes()
	{
		RecipeButtonList.GetChildren().ToList().ForEach(child => child.QueueFree());
		Database.Instance.UnlockedRecipes
			.OrderBy(recipe => recipe.Name)
			.Where(recipe => recipe.Category is "None" or "hands")
			.ToList().ForEach(AddRecipe);
	}
	
	private void AddRecipe(Recipe recipe)
	{
		var craftingItem = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_recipe_box.tscn").Instantiate<InventoryRecipeBox>();
		craftingItem.Init(_inventory, recipe);
		RecipeButtonList.AddChild(craftingItem);
	}
}
