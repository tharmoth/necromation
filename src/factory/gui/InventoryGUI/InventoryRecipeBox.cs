using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class InventoryRecipeBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_recipe_box.tscn");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory TargetInventory { get; set; } = new();
	private Recipe _recipe = new();

	// Static Accessor
	public static void UpdateRecipes(Inventory from, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());

		Database.Instance.UnlockedRecipes
			.OrderBy(recipe => recipe.Name)
			.Where(recipe => recipe.Category is "None" or "hands")
			.ToList().ForEach(recipe => InventoryRecipeBox.AddRecipe(from, recipe, list));
	}
	
	// Static Accessor
	private static void AddRecipe(Inventory from, Recipe recipe, Container list)
	{
		var craftingItem = Scene.Instantiate<InventoryRecipeBox>();
		craftingItem.Init(from, recipe);
		list.AddChild(craftingItem);
	}

	// Constructor workaround.
	private void Init(Inventory inventory, Recipe recipe)
	{
		TargetInventory = inventory;
		_recipe = recipe;
		ItemType = _recipe.Products.Keys.First();

		Button.GuiInput += @event =>
		{
			int numToCraft;
			switch (@event)
			{
				case InputEventMouseButton { Pressed: true } when Input.IsKeyPressed(Key.Shift):
					numToCraft = CountLabel.Text.ToInt();
					break;
				case InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true }:
					numToCraft = 5;
					break;
				case InputEventMouseButton { ButtonIndex: MouseButton.Middle, Pressed: true }:
					numToCraft = 10;
					break;
				case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }:
					numToCraft = 1;
					break;
				default:
					return;
			}
			Globals.FactoryScene.CraftingQueue.QueueRecipe(_recipe.Name, numToCraft);
		};
		
		TargetInventory.Listeners.Add(UpdateInventoryRecipeBox);
		UpdateInventoryRecipeBox();
		
		IngrediantsPopup.Register(_recipe, Button);
	}

	private void UpdateInventoryRecipeBox()
	{
		if (TargetInventory == null) return;
		Button.Disabled = !_recipe.CanCraft(TargetInventory);
		Icon.Modulate = _recipe.CanCraft(TargetInventory) ? Colors.White : new Color(.5f, .5f, .5f);
		var currentCraftableCount = int.MaxValue;
		foreach (var (item, count) in _recipe.Ingredients)
		{
			currentCraftableCount = Math.Min(currentCraftableCount, TargetInventory.CountItem(item) / count);
		}
		CountLabel.Text = currentCraftableCount.ToString();
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		TargetInventory.Listeners.Remove(UpdateInventoryRecipeBox);
	}
}
