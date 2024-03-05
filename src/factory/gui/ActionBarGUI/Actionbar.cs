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
		int index = 1;
		// Use the original recipe list for dumb sorting reasons.
		foreach (var recipe in Database.Instance.Recipes.Where(recipe => recipe.Category == _category)
			         .Where(recipe => Database.Instance.UnlockedRecipes.Contains(recipe)).Where(recipe => Building.IsBuilding(recipe.Products.FirstOrDefault().Key)))
		{
			var button = new ActionBarButton(recipe, index);
			AddChild(button);
			index++;
		}
	}
}
