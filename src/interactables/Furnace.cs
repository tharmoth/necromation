using Godot;
using System;
using System.Linq;
using Necromation.character;
using Necromation.gui;

public partial class Furnace : Node2D , RecipePopup.ICrafter
{
	public Recipe Recipe;
	
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
		Recipe?.Craft(Inventory.Instance);
		
	}

	private void RightClick()
	{
		GD.Print("Right Clicked!");
		GUI.Instance.Popup.DisplayPopup(this);
	}

	public void SetRecipe(Recipe recipe)
	{
		Recipe = recipe;
	}
}
