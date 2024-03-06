using Godot;
using System;
using Necromation;
using Necromation.gui;

public partial class ActionBarItem : PanelContainer
{
	
	private string _itemType = null;
	private Recipe _recipe;
	private int _index;
	string _cachedSelected = "";
	
	private Label CountLabel => GetNode<Label>("%CountLabel");
	private TextureRect Icon => GetNode<TextureRect>("%Icon");
	private Button _button => GetNode<Button>("%Button");
	private AudioStreamPlayer _audio => GetNode<AudioStreamPlayer>("%AudioStreamPlayer");
	
	public void SetFilter(string itemType)
	{
		_itemType = itemType;
		
		_audio.Play();
		
		Icon.Visible = true;
		Icon.Texture = Database.Instance.GetTexture(Globals.Player.Selected == _itemType ? "BoneHand" : _itemType);
		
		Globals.PlayerInventory.Listeners.Add(Update);
		Update();
	}



	public override void _Ready()
	{
		base._Ready();
		_index = GetParent().GetChildren().IndexOf(this);
		_button.GuiInput += ButtonPressed;

		ClearFilter();
	}
	
	private void ClearFilter()
	{
		_itemType = null;
		Icon.Visible = false;
		CountLabel.Text = "";
	}

	private void Update()
	{
		if (_itemType == null) return;
		CountLabel.Text = Globals.PlayerInventory.CountItem(_itemType).ToString();
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);

		if (_cachedSelected == Globals.Player.Selected || string.IsNullOrEmpty(_itemType)) return;
		_cachedSelected = Globals.Player.Selected;
		Icon.Visible = true;
		Icon.Texture = Database.Instance.GetTexture(Globals.Player.Selected == _itemType ? "BoneHand" : _itemType);
	}

	// Either select the item, show the filter menu, or clear the slot
	private void ButtonPressed(InputEvent @event)
	{
		if (@event is not InputEventMouseButton eventMouseButton || eventMouseButton.Pressed) return;
		
		if (eventMouseButton.ButtonIndex == MouseButton.Middle)
		{
			ClearFilter();
		} else if (eventMouseButton.ButtonIndex == MouseButton.Right || string.IsNullOrEmpty(_itemType))
		{
			FilterMenu.Display(this);
		} 
		else if (eventMouseButton.ButtonIndex == MouseButton.Left && Globals.PlayerInventory.CountItem(_itemType) > 0 )
		{
			Globals.Player.Selected = _itemType;
		} 
	}
	
	// Allow the player to select the item by pressing the number key
	public override void _UnhandledKeyInput(InputEvent @event)
	{
		base._UnhandledKeyInput(@event);
		if (@event is InputEventKey { Pressed: true } eventKey && eventKey.Keycode == Key.Key0 + _index + 1)
		{
			Globals.Player.Selected = _itemType;
		}
	}
	
	
	public override void _ExitTree()
	{
		base._ExitTree();
		Globals.PlayerInventory.Listeners.Remove(Update);
	}
}
