using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class RecipeSelectionGui : Control
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/RecipeSelectionGui/RecipeSelectionGui.tscn");

	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container RecipeList => GetNode<Container>("%RecipeList");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/

	// Static Accessor
	public static void Display(Inventory targetInventory, ICrafter crafter)
	{
		var gui =Scene.Instantiate<RecipeSelectionGui>();
		gui.Init(targetInventory, crafter);
		Globals.FactoryScene.Gui.Open(gui);
	}

	// Constructor workaround.
	private void Init(Inventory targetInventory, ICrafter crafter)
	{
		RecipeSelectionItemBox.UpdateRecipes(targetInventory, crafter, RecipeList);
	}
}
