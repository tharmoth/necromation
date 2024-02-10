using Godot;
using System;
using System.Linq;
using Necromation.character;
using Necromation.gui;

public partial class Furnace : Node2D , ICrafter, ProgressTracker.IProgress, BuildingTileMap.IBuilding
{
	private Recipe _recipe;
	private Inventory _inputInventory = new Inventory();
	private Inventory _outputInventory = new Inventory();
	private float _time;

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (_recipe == null || !_recipe.CanCraft(_inputInventory))
		{
			_time = 0;
			return;
		}
		
		_time += (float)delta;

		if (GetProgressPercent() < 1.0f) return;
		_time = 0;
		_recipe.Craft(_inputInventory, _outputInventory);
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
		if (_recipe == null)
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
		return _recipe;
	}

	public void SetRecipe(Recipe recipe)
	{
		_recipe = recipe;
	}

	public Inventory GetInputInventory()
	{
		return _inputInventory;
	}

	public Inventory GetOutputInventory()
	{
		return _outputInventory;
	}

	public float GetProgressPercent()
	{
		if (_recipe == null) return 0;
		return _time / _recipe.Time;
	}

	public string ItemType => "Stone Furnace";
}
