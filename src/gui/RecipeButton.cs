using Godot;
using System.Collections.Generic;
using Necromation.gui;

public partial class RecipeButton : PanelContainer
{
	[Export] private PackedScene _buildingScene;
	
	private string _itemType;
	public string ItemType
	{
		get => _itemType;
		set
		{
			_itemType = value;
			GetNode<Button>("Button").Text = ItemType;
		}
	}
	public Recipe Recipe { get; set; }
	
	private bool _buildMode;
	
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("Button").GuiInput += _onButtonPressed2;
		GetNode<Button>("Button").Text = ItemType;
        
		Inventory.Instance.Listeners.Add(Update);
		
	}
	
	private void Update()
	{
		GetNode<Label>("Label").Text = Inventory.Instance.CountItem(ItemType).ToString();
	}
	
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		
		if (@event is not InputEventMouseButton eventMouseButton) return;
		if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Right) _buildMode = false;
		if (!eventMouseButton.Pressed || eventMouseButton.ButtonIndex != MouseButton.Left) return;
		if (!_buildMode) return;
		if (Inventory.Instance.CountItem(ItemType) < 1) return;
		
		
		_buildMode = false;
		Inventory.Instance.RemoveItem(ItemType);

		var building = _buildingScene.Instantiate<Node2D>();
		GetTree().Root.AddChild(building);
		building.GlobalPosition = building.GetGlobalMousePosition();
	}

	private void _onButtonPressed2(InputEvent @event)
	{
		if (@event is not InputEventMouseButton eventMouseButton || eventMouseButton.Pressed) return;
		if (eventMouseButton.ButtonIndex == MouseButton.Left)
		{
			_buildMode = true;
		}
		else if (eventMouseButton.ButtonIndex == MouseButton.Right)
		{
			Recipe.Craft(Inventory.Instance);
		}
	}
}
