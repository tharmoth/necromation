using Godot;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class RecipePopup : Control
{
	[Export] private string _category = "None";

	private ICrafter _crafter;
	
	public void DisplayPopup(ICrafter crafter)
	{
		_category = crafter.GetCategory();
		GlobalPosition = GetViewport().GetMousePosition() + new Vector2(40, 0);
		Visible = true;
		_crafter = crafter;

		GetNode("RecipeList").GetChildren().OfType<Button>().ToList().ForEach(button =>
		{
			button.GetParent().RemoveChild(button);
			button.QueueFree();
		});
		AddButtons();
	}
	
	public void HidePopup()
	{
		Visible = false;
	}
	
	public void SetRecipe(Recipe recipe)
	{
		Visible = false;
		_crafter.SetRecipe(recipe);
	}
	
	private void AddButtons()
	{
		foreach (var recipe in Globals.Database.UnlockedRecipes.Where(recipe => recipe.Category == _category))
		{
			var button = new Button();
			button.CustomMinimumSize = new Vector2(100, 30);
			button.Pressed += () =>
			{
				GUI.Instance.Popup.Visible = false;
				SetRecipe(recipe);
				GD.Print("Button pressed");
			};
			button.Text = recipe.Name;
			GetNode("RecipeList").AddChild(button);
		}
	}
}
