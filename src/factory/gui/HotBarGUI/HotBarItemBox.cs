using Godot;
using System;
using System.Linq;
using System.Xml.Linq;
using Necromation;
using Necromation.gui;

public partial class HotBarItemBox : ItemBox
{
	private int _index;
	private string _cachedSelected = "";
	
	private AudioStreamPlayer Audio => GetNode<AudioStreamPlayer>("%AudioStreamPlayer");
	
	public void SetFilter(string itemType)
	{
		if (string.IsNullOrEmpty(itemType))
		{
			ClearFilter();
			return;
		}
		
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
	
	#region Save/Load
	/******************************************************************
	 * Save/Load                                                      *
	 ******************************************************************/
	public static Godot.Collections.Dictionary<string, Variant> Save()
	{
		var dict = new Godot.Collections.Dictionary<string, Variant>
		{
			["ItemType"] = "HotBar"
		};

		var items = Globals.FactoryScene.Gui.HotBar.GetChildren().OfType<HotBarItemBox>().ToList();
		for (var i = 0; i < items.Count; i++)
		{
			dict["HotBarItem" + i] = items[i].SaveItem();
		}
		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		int index = 0;
		while (nodeData.ContainsKey("HotBarItem" + index))
		{
			var data = (Godot.Collections.Dictionary<string, Variant>) nodeData["HotBarItem" + index];
			var itemIndex = data["Index"].AsInt32();
			Globals.FactoryScene.Gui.HotBar.GetChildren()
				.OfType<HotBarItemBox>()
				.First(itemBox => itemBox._index == itemIndex)
				.SetFilter(data["ItemFilter"].ToString());
			index++;
		}
	}

	private Godot.Collections.Dictionary<string, Variant> SaveItem()
	{
		return new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", "HotBarItemBox" },
			{ "Index", _index },
			{ "ItemFilter", ItemType }
		};
	}
	#endregion
}
