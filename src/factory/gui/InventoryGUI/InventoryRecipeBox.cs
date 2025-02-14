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
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGui/InventoryRecipeBox.tscn");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory TargetInventory { get; set; } = new();
	private Recipe _recipe = new();

	// Static Accessor
	public static void UpdateRecipes(Inventory from, Container list)
	{
		AddMissing(from, list);
		RemoveExtra(list);
		var sorted = list.GetChildren().OfType<InventoryRecipeBox>()
			.ToList();
		sorted.Sort((a, b) 
			=> Database.Instance.CompareItems(a.ItemType, b.ItemType));
		sorted.ForEach(item => list.MoveChild(item, sorted.IndexOf(item)));
	}
	
	// Static Accessor
	private static void AddRecipe(Inventory from, Recipe recipe, Container list)
	{
		var craftingItem = Scene.Instantiate<InventoryRecipeBox>();
		craftingItem.Init(from, recipe);
		list.AddChild(craftingItem);
	}
	
	private static void AddMissing(Inventory from, Container list)
	{
		Database.Instance.UnlockedRecipes
			.OrderBy(recipe => recipe.Name)
			// .Where(recipe => recipe.Category is "None" or "hands")
			.Where(recipe => list.GetChildren().OfType<InventoryRecipeBox>().FirstOrDefault(box => box._recipe == recipe) == null)
			.ToList().ForEach(recipe => AddRecipe(from, recipe, list));
	}

	private static void RemoveExtra(Container list)
	{
		list.GetChildren().OfType<InventoryRecipeBox>()
			.Where(recipeBox => !Database.Instance.UnlockedRecipes.Contains(recipeBox._recipe))
			.ToList()
			.ForEach(item => item.QueueFree());
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
			Globals.FactoryScene.CraftingQueue.QueueRecipe(_recipe, numToCraft);
		};
		
		TargetInventory.Listeners.Add(UpdateInventoryRecipeBox);
		UpdateInventoryRecipeBox();

		IngrediantsPopup.Register(_recipe, Button);
	}

	private void UpdateInventoryRecipeBox()
	{
		if (TargetInventory == null) return;
		var currentCraftableCount = _recipe.GetMaxCraftable(TargetInventory);
		
		Button.Disabled = currentCraftableCount == 0;
		Icon.Modulate = currentCraftableCount == 0 ? new Color(.5f, .5f, .5f) : Colors.White;
		CountLabel.Text = currentCraftableCount.ToString();
		ColorRect.Visible = currentCraftableCount == 0;
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		TargetInventory.Listeners.Remove(UpdateInventoryRecipeBox);
	}
}
