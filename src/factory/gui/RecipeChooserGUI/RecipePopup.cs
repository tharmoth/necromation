using Godot;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class RecipePopup : Control
{
	[Export] private string _category = "None";
	private Container RecipeList => GetNode<Container>("%RecipeList");
	
	private ICrafter _crafter;
	// The inventory items will be dumped to if a recipe is selected
	private Inventory _targetInventory;
	
	public static void Display(Inventory targetInventory, ICrafter crafter)
	{
		var gui = GD.Load<PackedScene>("res://src/factory/gui/RecipeChooserGUI/recipe_popup.tscn").Instantiate<RecipePopup>();
		gui.Init(targetInventory, crafter);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	private void Init(Inventory targetInventory, ICrafter crafter)
	{
		_targetInventory = targetInventory;
		_crafter = crafter;
		_category = _crafter.GetCategory();
		RecipeList.GetChildren().OfType<Button>().ToList().ForEach(button => button.QueueFree());
		AddButtons();
	}

	public override void _Ready()
	{
		base._Ready();
		GlobalPosition = GetViewport().GetMousePosition() + new Vector2(40, 0);
		FactoryGUI.SnapToScreen(this);
	}
	
	private void AddButtons()
	{
		foreach (var recipe in Database.Instance.UnlockedRecipes.Where(recipe => recipe.Category == _category))
		{
			var button = new Button();
			button.CustomMinimumSize = new Vector2(100, 30);
			button.Pressed += () =>
			{
				_crafter.SetRecipe(_targetInventory, recipe);
				QueueFree();
			};
			button.Text = recipe.Name;
			button.AddChild(GD.Load<PackedScene>("res://src/shared/gui/outline.tscn").Instantiate<Control>());
			RecipeList.AddChild(button);
			IngrediantsPopup.Register(recipe, button);
		}
	}
}
