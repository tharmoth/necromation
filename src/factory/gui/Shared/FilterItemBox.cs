using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class FilterItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/factory/gui/Shared/FilterItemBox.tscn");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _sourceInventory;
	private List<Inventory> _to;
	private List<string> _items;
	
	// Static Accessor
	public static void AddItemBox(Inventory from, List<Inventory> to, List<String> items, Container list)
	{
		var inventoryItem = ItemScene.Instantiate<FilterItemBox>();
		inventoryItem.Init(from, to, items);
		list.AddChild(inventoryItem);
	}	
	
	private void Init(Inventory source, List<Inventory> to, List<string> items)
	{
		_sourceInventory = source;
		_to = to;
		_items = items;
		CountLabel.Text = "";
		ItemType = null;

		if (items.Count > 0)
		{
			ItemPopup.Register(Button, "Possible Inputs:", items);
		}

		Button.Pressed += ButtonPress;
	}

	/// <summary>
	/// <para>When a crafter item box is pressed it will do one of two things:</para>
	/// 1. If the player has the item selected, it will transfer the item to the source inventory and deselect the item.<br />
	/// 2. If the player does not have the item selected, it will transfer the item from the source inventory to the
	///    first inventory in the list of inventories to transfer to that will accept the item.<br />
	/// </summary>
	private void ButtonPress()
	{
		var selected = Globals.Player.Selected;
		if (_items.Contains(selected)) TransferToPlayer(_sourceInventory, selected);
	}
}
