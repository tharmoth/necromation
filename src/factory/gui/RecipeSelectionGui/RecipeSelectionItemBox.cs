using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class RecipeSelectionItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/RecipeSelectionGui/RecipeSelectionItemBox.tscn");
	
	// Static Accessor
	public static void UpdateRecipes(Inventory dumpInventory, ICrafter crafter, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());
		
		Database.Instance.UnlockedRecipes.Where(recipe => crafter.GetCategory() == recipe.Category)
			.OrderBy(recipe => recipe.Name)
			.ToList()
			.ForEach(recipe => AddRecipe(dumpInventory, recipe, crafter, list));
	}
	
	// Static Accessor
	private static void AddRecipe(Inventory dumpInventory, Recipe recipe, ICrafter crafter, Container list)
	{
		var craftingItem = Scene.Instantiate<RecipeSelectionItemBox>();
		craftingItem.Init(dumpInventory, recipe, crafter);
		list.AddChild(craftingItem);
	}

	private void Init(Inventory dumpInventory, Recipe recipe, ICrafter crafter)
	{
		ItemType = recipe.Products.First().Key;
		CountLabel.Visible = false;
		Button.Pressed += () =>
		{
			crafter.SetRecipe(dumpInventory, recipe);
			Globals.FactoryScene.Gui.CloseGui();
		};
		
		IngrediantsPopup.Register(recipe, Button);
	}
}
