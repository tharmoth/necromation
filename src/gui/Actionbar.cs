using Godot;
using System;
using Necromation.character;

public partial class Actionbar : HBoxContainer
{
	public override void _Ready()
	{
		AddMissingItems();
	}
	
	private void AddMissingItems()
	{
		foreach (var item in Database.Instance.Recipes)
		{
			var button = GD.Load<PackedScene>("res://src/gui/recipe_button.tscn").Instantiate<RecipeButton>();
			button.ItemType = item.Name;
			GD.Print(item);
			button.Recipe = item;
			AddChild(button);
		}
	}
}
