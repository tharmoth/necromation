using Godot;
using System.Collections.Generic;
using Necromation;
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("Button").GuiInput += ButtonPressed;
		GetNode<Button>("Button").Text = ItemType;
        
		Globals.PlayerInventory.Listeners.Add(Update);

		CraftingListPopup.Register(Recipe, GetNode<Button>("Button"));
	}

	private void Update()
	{
		GetNode<Label>("Label").Text = Globals.PlayerInventory.CountItem(ItemType).ToString();
	}

	private void ButtonPressed(InputEvent @event)
	{
		if (@event is not InputEventMouseButton eventMouseButton || eventMouseButton.Pressed) return;
		if (eventMouseButton.ButtonIndex == MouseButton.Left && Globals.PlayerInventory.CountItem(ItemType) > 0 )
		{
			Globals.Player.Selected = ItemType;
		}
		else if (eventMouseButton.ButtonIndex == MouseButton.Right)
		{
			Recipe.Craft(Globals.PlayerInventory);
		}
	}
}
