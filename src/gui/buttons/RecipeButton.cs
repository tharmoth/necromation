using Godot;
using Necromation;
using Necromation.gui;

public partial class RecipeButton : PanelContainer
{
	private readonly string _itemType;
	private readonly Recipe _recipe;
	private readonly Button _button = new();
	private readonly Label _label = new();
	
	
	public override void _ExitTree()
	{
		base._ExitTree();
		Globals.PlayerInventory.Listeners.Remove(Update);
	}

	public RecipeButton(Recipe recipe)
	{
		_recipe = recipe;
		_itemType = recipe.Name;
		
		_button.GuiInput += ButtonPressed;
		_button.CustomMinimumSize = new Vector2(100, 100);
		_button.ButtonMask = MouseButtonMask.Left | MouseButtonMask.Right;
		_button.Text = _itemType;
		AddChild(_button);

		_label.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
		_label.SizeFlagsVertical = SizeFlags.ShrinkBegin;
		AddChild(_label);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Globals.PlayerInventory.Listeners.Add(Update);
		Update();

		CraftingListPopup.Register(_recipe, _button);
	}

	private void Update()
	{
		_label.Text = Globals.PlayerInventory.CountItem(_itemType).ToString();
	}

	private void ButtonPressed(InputEvent @event)
	{
		if (@event is not InputEventMouseButton eventMouseButton || eventMouseButton.Pressed) return;
		if (eventMouseButton.ButtonIndex == MouseButton.Left && Globals.PlayerInventory.CountItem(_itemType) > 0 )
		{
			Globals.Player.Selected = _itemType;
		}
		else if (eventMouseButton.ButtonIndex == MouseButton.Right)
		{
			_recipe.Craft(Globals.PlayerInventory);
		}
	}
}
