using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class RecipeButton : PanelContainer
{
	private TextureRect Icon => GetNode<TextureRect>("%RecipeIcon");
	public Button Button => GetNode<Button>("%Button");

	public Inventory TargetInventory { get; set; } = new();
	
	private Recipe _recipe = new();
	public Recipe Recipe
	{
		get => _recipe;
		set
		{
			_recipe = value;
			Icon.Texture = _recipe.GetIcon();
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		TargetInventory.Listeners.Remove(Update);
	}

	public override void _Ready()
	{
		base._Ready();
		Button.Pressed += () => _recipe.Craft(TargetInventory);
		Update();
		TargetInventory.Listeners.Add(Update);
		IngrediantsPopup.Register(_recipe, Button);
	}
	
	private void Update()
	{
		Button.Disabled = !_recipe.CanCraft(TargetInventory);
		Icon.Modulate = _recipe.CanCraft(TargetInventory) ? Colors.White : new Color(.3f, .3f, .3f);
	}
}
