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
			popup = CraftingListPopup.DisplayPopup(recipe);
		};
		control.MouseExited += () =>
		{
			if (!IsInstanceValid(popup)) return;
			popup.QueueFree();
		};
	}
	
	public static CraftingListPopup DisplayPopup(Recipe recipe)
	{
		var popup = (CraftingListPopup) GD.Load<PackedScene>("res://src/gui/lists/crafting_list_popup.tscn").Instantiate();
		popup._recipe = recipe;
		GUI.Instance.AddChild(popup);
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

		SnapToScreen(this);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		SnapToScreen(this);
	}

	private void AddRow(string name, int amount)
	{
		var label = new Label();
		label.Text = amount + " x " + name;
		
		var texture = new TextureRect();
		texture.Texture = Globals.Database.GetTexture(name);
		
		var row = new HBoxContainer();
		row.AddChild(texture);
		row.AddChild(label);
		GetNode<VBoxContainer>("%Rows").AddChild(row);
	}
	
	/*
	 * Adjusts the position of the popup so that it is always visible on the screen
	 */
	private static void SnapToScreen(Control node)
	{
		node.ResetSize();
		
		node.GlobalPosition = node.GetViewport().GetMousePosition() + new Vector2(40, 0);
		
		// Ensure the PopupMenu is not partially off-screen
		var screenSize = node.GetViewportRect().Size;
		
		// Check if the PopupMenu exceeds the right edge of the screen move it to the left of the cursor
		if (node.GlobalPosition.X + node.Size.X > screenSize.X)
		{
			node.GlobalPosition = new Vector2(node.GetViewport().GetMousePosition().X - node.Size.X - 40, node.GlobalPosition.Y);
		}
		
		// Check if the PopupMenu exceeds the bottom edge of the screenmove it to the top of the cursor
		if (node.GlobalPosition.Y + node.Size.Y > screenSize.Y)
		{
			node.GlobalPosition = new Vector2(node.GlobalPosition.X, screenSize.Y - node.Size.Y);
		}
	}
}
