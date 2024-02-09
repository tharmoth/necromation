using Godot;
using System;
using System.Linq;
using Necromation.character;
using Necromation.gui;

public partial class Furnace : Node2D , ICrafter
{
	public Recipe Recipe;
	private Inventory _inputInventory = new Inventory();
	private Inventory _outputInventory = new Inventory();


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Recipe?.CanCraft(_inputInventory) == true)
		{
			Recipe.Craft(_inputInventory, _outputInventory);
		}
	}

	public override void _Input(InputEvent @event)
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		if (@event is not InputEventMouseButton eventMouseButton) return;
		if (!sprite.GetRect().HasPoint(sprite.ToLocal(GetGlobalMousePosition()))) return;
		if (!eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left) LeftClick();
	}

	private void LeftClick()
	{
		if (Recipe == null)
		{
			GUI.Instance.Popup.DisplayPopup(this);
		}
		else
		{
			GUI.Instance.CrafterGui.Display(this);
		}
	}

	public Recipe GetRecipe(Recipe recipe)
	{
		return Recipe;
	}

	public void SetRecipe(Recipe recipe)
	{
		Recipe = recipe;
	}

	public Inventory GetInputInventory()
	{
		return _inputInventory;
	}

	public Inventory GetOutputInventory()
	{
		return _outputInventory;
	}
}
