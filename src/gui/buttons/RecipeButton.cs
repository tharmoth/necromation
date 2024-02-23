using Godot;
using Necromation;
using Necromation.gui;

public partial class RecipeButton : PanelContainer
{
	private readonly string _itemType;
	private readonly Recipe _recipe;
	private readonly Button _button = new();
	private readonly Label _countLabel = new();
	private readonly Label _nameLabel = new();
	private readonly TextureRect _icon = new();
	private readonly int _index;
	
	public override void _ExitTree()
	{
		base._ExitTree();
		Globals.PlayerInventory.Listeners.Remove(Update);
	}

	public RecipeButton(Recipe recipe, int index)
	{
		_recipe = recipe;
		_itemType = recipe.Name;
		_index = index;
		CustomMinimumSize = new Vector2(100, 100);
		
		VBoxContainer vbox = new();
		vbox.SizeFlagsHorizontal = SizeFlags.Fill;
		vbox.SizeFlagsVertical = SizeFlags.Fill;
		vbox.MouseFilter = MouseFilterEnum.Ignore;
		vbox.AddThemeConstantOverride("separation", -10);
		
		_button.GuiInput += ButtonPressed;
		_button.ButtonMask = MouseButtonMask.Left | MouseButtonMask.Right;
		_button.SizeFlagsVertical = SizeFlags.Fill;
		_button.SizeFlagsHorizontal = SizeFlags.Fill;
		
		_countLabel.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
		_countLabel.SizeFlagsVertical = SizeFlags.ShrinkBegin;
		_countLabel.MouseFilter = MouseFilterEnum.Ignore;
		
		_icon.Texture = Globals.Database.GetTexture(_itemType);
		_icon.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
		_icon.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		_icon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
		_icon.MouseFilter = MouseFilterEnum.Ignore;
		_icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		_icon.CustomMinimumSize = new Vector2(96, 96);
		
		_nameLabel.Text = _itemType;
		_nameLabel.SizeFlagsVertical = SizeFlags.ShrinkEnd;
		_nameLabel.SizeFlagsHorizontal = SizeFlags.Fill;
		_nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_nameLabel.MouseFilter = MouseFilterEnum.Ignore;

		vbox.AddChild(_countLabel);
		vbox.AddChild(_icon);
		vbox.AddChild(_nameLabel);
		
		AddChild(_button);
		AddChild(vbox);
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		base._UnhandledKeyInput(@event);
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Key0 + _index)
		{
			Globals.Player.Selected = _itemType;
		}
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
		_countLabel.Text = Globals.PlayerInventory.CountItem(_itemType).ToString();
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
