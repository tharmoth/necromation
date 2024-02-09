using Godot;
using System;
using System.Linq;
using Necromation.character;
using Necromation.gui;

public partial class Furnace : Node2D , RecipePopup.ICrafter
{
	public Recipe Recipe;
	private Inventory _inventory = new Inventory();


	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (Recipe?.CanCraft(_inventory) == true) Recipe.Craft(_inventory);
	}

	public override void _Input(InputEvent @event)
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		if (@event is not InputEventMouseButton eventMouseButton) return;
		if (!sprite.GetRect().HasPoint(sprite.ToLocal(GetGlobalMousePosition()))) return;
		if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left) LeftClick();
		if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Right) RightClick();
	}

	private void LeftClick()
	{
		GD.Print("Clicked!");
		Recipe?.Craft(_inventory);
		
	}

	private void RightClick()
	{
		GD.Print("Right Clicked!");
		if (Recipe == null)
		{
			GUI.Instance.Popup.DisplayPopup(this);
		}
		else
		{
			GUI.Instance.ContainerGui.Display(_inventory);
		}
		
	}

	public void SetRecipe(Recipe recipe)
	{
		Recipe = recipe;
	}
}
