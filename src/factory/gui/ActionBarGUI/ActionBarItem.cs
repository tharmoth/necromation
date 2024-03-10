using Godot;
using System;
using Necromation;
using Necromation.gui;

public partial class ActionBarItem : ItemBox
{
	private int _index;
	private string _cachedSelected = "";
	
	private AudioStreamPlayer Audio => GetNode<AudioStreamPlayer>("%AudioStreamPlayer");
	
	public void SetFilter(string itemType)
	{
		// Update data
		ItemType = itemType;

		// Update Visuals
		UpdateCount();
		
		// Play sound
		Audio.Play();
		
		ItemPopup.Register(itemType, Button);
	}

	public override void _Ready()
	{
		base._Ready();
		_index = GetParent().GetChildren().IndexOf(this);
		Button.GuiInput += ButtonPressed;
		
		if (!Globals.PlayerInventory.Listeners.Contains(UpdateCount)) Globals.PlayerInventory.Listeners.Add(UpdateCount);

		ClearFilter();
	}
	
	private void ClearFilter()
	{
		ItemType = null;
		CountLabel.Text = "";
	}

	private void UpdateCount()
	{
		if (ItemType != null) CountLabel.Text = Globals.PlayerInventory.CountItem(ItemType).ToString();
	}
	
	protected override void UpdateIcon()
	{
		if (Globals.Player.Selected == ItemType && !string.IsNullOrEmpty(ItemType))
		{
			Icon.Visible = true;
			Icon.Texture = Database.Instance.GetTexture("BoneHand");
			return;
		}
		base.UpdateIcon();
	}
	
	// Switch the icon to the BoneHand when the item is selected.
	public override void _Process(double delta)
	{
		base._Process(delta);

		if (_cachedSelected == Globals.Player.Selected || string.IsNullOrEmpty(ItemType)) return;
		_cachedSelected = Globals.Player.Selected;
		UpdateIcon();
	}

	// Either select the item, show the filter menu, or clear the slot
	private void ButtonPressed(InputEvent @event)
	{
		if (@event is not InputEventMouseButton eventMouseButton || eventMouseButton.Pressed) return;
		
		if (eventMouseButton.ButtonIndex == MouseButton.Middle)
		{
			ClearFilter();
		} 
		else if (eventMouseButton.ButtonIndex == MouseButton.Right || string.IsNullOrEmpty(ItemType))
		{
			if (Globals.Player.Selected != null)
			{
				SetFilter(Globals.Player.Selected);
				Globals.Player.Selected = null;
			}
			else
			{
				ClearFilter();
				FilterMenu.Display(this);
			}
		} 
		else if (eventMouseButton.ButtonIndex == MouseButton.Left && Globals.PlayerInventory.CountItem(ItemType) > 0 )
		{
			Globals.Player.Selected = ItemType;
		}
	}
	
	// Allow the player to select the item by pressing the number key
	public override void _UnhandledKeyInput(InputEvent @event)
	{
		base._UnhandledKeyInput(@event);
		if (_index >= 9) return;
		if (@event is InputEventKey { Pressed: true } eventKey 
		    && eventKey.Keycode == Key.Key0 + _index + 1 
		    && Globals.PlayerInventory.CountItem(ItemType) > 0)
		{
			Globals.Player.Selected = ItemType;
		}
	}
	
	// Clean up the event listeners when the node is freed
	public override void _ExitTree()
	{
		base._ExitTree();
		Globals.PlayerInventory.Listeners.Remove(UpdateCount);
	}
}
