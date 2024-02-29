using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class CraftingListPopup : PanelContainer
{
	public static void Register(Recipe recipe, Control control)
	{
		CraftingListPopup popup = null;
		control.MouseEntered += () =>
		{
			if (IsInstanceValid(popup)) return;
			popup = DisplayPopup(recipe);
		};
		control.MouseExited += () =>
		{
			if (!IsInstanceValid(popup)) return;
			popup.QueueFree();
		};
		control.VisibilityChanged += () =>
		{
			if (!IsInstanceValid(popup)) return;
			popup.QueueFree();
		};
		control.TreeExited += () =>
		{
			if (!IsInstanceValid(popup)) return;
			popup.QueueFree();
		};
	}
	
	public static CraftingListPopup DisplayPopup(Recipe recipe)
	{
		FactoryGUI.Instance.GetChildren().OfType<CraftingListPopup>().ToList().ForEach(popup => popup.QueueFree());
		var popup = (CraftingListPopup) GD.Load<PackedScene>("res://src/gui/lists/crafting_list_popup.tscn").Instantiate();
		popup._recipe = recipe;
		FactoryGUI.Instance.AddChild(popup);
		return popup;
	}

	private Recipe _recipe;

	public override void _Ready()
	{
		if (_recipe == null) throw new ArgumentException("Recipe popup recipe cannot be null when added to tree");

		Size = Vector2.Zero;

		GetNode<Label>("%RecipeNameLabel").Text = _recipe.Name;
		GetNode<Label>("%CraftingTimeLabel").Text = _recipe.Time + "s Crafting Time";
		
		GetNode<VBoxContainer>("%Rows").GetChildren().ToList().ForEach(node => node.Free());
		_recipe.Ingredients.ToList().ForEach(ingredient => AddRow(ingredient.Key, ingredient.Value));

		FactoryGUI.SnapToScreen(this);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		FactoryGUI.SnapToScreen(this);
	}

	private void AddRow(string name, int amount)
	{
		var label = new Label();
		label.Text = amount + " x " + name;
		
		var texture = new TextureRect();
		texture.Texture = Globals.Database.GetTexture(name);
		texture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		texture.CustomMinimumSize = new Vector2(32, 32);
		
		var row = new HBoxContainer();
		row.AddChild(texture);
		row.AddChild(label);
		GetNode<VBoxContainer>("%Rows").AddChild(row);
	}
}
