using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class InventoryItem : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_item_box.tscn");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _targetInventory;
	private Inventory _sourceInventory;
	private string _cachedSelected = "";
	
	// Static Accessor
	public static void UpdateInventory(Inventory from, Inventory to, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());
		from.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(item => InventoryItem.AddInventoryItem(item, from, to, list));
	}
	
	// Static Accessor
	private static void AddInventoryItem(KeyValuePair<string, int> item, Inventory from, Inventory to, Container list)
	{
		var inventoryItem = ItemScene.Instantiate<InventoryItem>();
		inventoryItem.Init(from, to, item.Key, item.Value);
		list.AddChild(inventoryItem);
	}
	
	private void Init(Inventory source, Inventory target, string item, int count)
	{
		ItemType = item;
		CountLabel.Text = count.ToString();
		_targetInventory = target;
		_sourceInventory = source;
		
		Button.Pressed += () =>
		{
			if (_targetInventory == null || _sourceInventory == null)
			{
				Globals.Player.Selected = ItemType;
				return;
			}
			var sourceCount = Input.IsActionPressed("shift") ? _sourceInventory.Items[ItemType] : 1;
			var targetCapacity = _targetInventory.GetMaxTransferAmount(ItemType);
			var amountToTransfer = Mathf.Min(sourceCount, targetCapacity);

			Inventory.TransferItem(_sourceInventory, _targetInventory, ItemType, amountToTransfer);
		};
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
}
