using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.character;

public partial class Actionbar : Control
{
	[Export] private string _category = "None";
	[Export] private PackedScene _buttonScene = GD.Load<PackedScene>("res://src/gui/buttons/recipe_button.tscn");
	
	public override void _Ready()
	{
		AddMissingItems();
	}
	
	private void AddMissingItems()
	{
		foreach (var item in Globals.Database.Recipes.Where(recipe => recipe.Category == _category))
		{
			var button = _buttonScene.Instantiate<RecipeButton>();
			button.ItemType = item.Name;
			button.Recipe = item;
			AddChild(button);
		}
	}
}
