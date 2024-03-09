using Godot;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class RecipePopup : Control
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/RecipeChooserGUI/recipe_popup.tscn");
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/shared/gui/outline.tscn");

	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container RecipeList => GetNode<Container>("%RecipeList");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private ICrafter _crafter;
	// The inventory items will be dumped to if a recipe is selected
	private Inventory _targetInventory;
	
	private string _category = "None";
	
	// Static Accessor
	public static void Display(Inventory targetInventory, ICrafter crafter)
	{
		var gui =Scene.Instantiate<RecipePopup>();
		gui.Init(targetInventory, crafter);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	// Constructor workaround.
	private void Init(Inventory targetInventory, ICrafter crafter)
	{
		_targetInventory = targetInventory;
		_crafter = crafter;
		_category = _crafter.GetCategory();
		RecipeList.GetChildren().ToList().ForEach(child => child.QueueFree());
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
			button.AddChild(ItemScene.Instantiate<Control>());
			RecipeList.AddChild(button);
			IngrediantsPopup.Register(recipe, button);
		}
	}
}
