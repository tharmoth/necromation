using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class ItemSelectionItemBox : ItemBox
{
	public void Init(Inventory dumpInventory, ICrafter crafter)
	{
		ItemType = crafter.GetRecipe().Products.First().Key;
		IngrediantsPopup.Register(crafter.GetRecipe(), Button);
		CountLabel.Visible = false;
		Button.Pressed += () =>
		{
			RecipeSelectionGui.Display(dumpInventory, crafter);
		};
	}
}
