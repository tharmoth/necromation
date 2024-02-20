using Godot;
using System.Linq;
using Necromation;

public partial class Actionbar : Control
{
	[Export] private string _category = "None";

	public override void _Ready()
	{
		Globals.ResearchListeners.Add(Update);
		Update();
	}

	private void Update()
	{
		GetChildren().ToList().ForEach(node => node.Free());
		AddMissingItems();
	}
	
	private void AddMissingItems()
	{
		// Use the original recipe list for dumb sorting reasons.
		foreach (var item in Globals.Database.Recipes.Where(recipe => recipe.Category == _category)
			         .Where(recipe => Globals.Database.UnlockedRecipes.Contains(recipe)))
		{
			var button = new RecipeButton(item);
			AddChild(button);
		}
	}
}
