using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class ItemBox : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/Shared/ItemBox.tscn");
	
	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	protected TextureRect Icon => GetNode<TextureRect>("%Icon");
	protected Label CountLabel => GetNode<Label>("%CountLabel");
	protected Button Button => GetNode<Button>("%Button");
	protected ProgressBar ProgressBar => GetNode<ProgressBar>("%ProgressBar");
	protected ColorRect ColorRect => GetNode<ColorRect>("%ColorRect");
	
	private string _itemType;
	public string ItemType
	{
		get => _itemType;
		set
		{
			_itemType = value;
			UpdateIcon();
		}
	}

	protected virtual void UpdateIcon()
	{
		if (_itemType == null)
		{
			Icon.Visible = false;
			ItemPopup.Unregister(Button);
		}
		else
		{
			Icon.Visible = true;
			Icon.Texture = Database.Instance.GetTexture(_itemType);
			ItemPopup.Register(ItemType, Button);
		}
	}
	
	// Static Accessor
	public static void UpdateInventory(Inventory from, Container list)
	{
		// Free instead of queuefree so that MapSquad can detect if empty.
		list.GetChildren().ToList().ForEach(child => child.Free());
		from.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(item => AddItem(item, list));
	}

	// Static Accessor
	private static void AddItem(KeyValuePair<string, int> item, Container list)
	{
		var inventoryItem = Scene.Instantiate<ItemBox>();
		inventoryItem.Init(item.Key, item.Value);
		list.AddChild(inventoryItem);
	}

	protected void Init(string item, int count)
	{
		ItemType = item;
		CountLabel.Text = count.ToString();
	}
	
	/************************************************************************
	 * Useful actions commonly taken by item boxes							*
	 ************************************************************************/
	public static void TransferToPlayer(Inventory sourceInventory, string selected)
	{
		var playerCount = Input.IsActionJustReleased("right_click") ? 
			1 : 
			Globals.PlayerInventory.CountItem(selected);
		var targetCapacity = sourceInventory.GetMaxTransferAmount(selected);
		var amountToTransfer = Mathf.Min(playerCount, targetCapacity);
		
		if (amountToTransfer > 0) Inventory.TransferItem(Globals.PlayerInventory, sourceInventory, selected, amountToTransfer);
		if (Globals.PlayerInventory.CountItem(selected) == 0) Globals.Player.Selected = null;
	}

	public static void TransferToSource(Inventory sourceInventory, List<Inventory> to, string itemType)
	{
		int sourceCount;
		if (Input.IsActionJustReleased("right_click")) sourceCount = Mathf.CeilToInt(sourceInventory.CountItem(itemType) / 2.0f);
		else if (Input.IsActionPressed("shift")) sourceCount = sourceInventory.CountItem(itemType);
		else if (Input.IsActionPressed("control")) sourceCount = Math.Min(5, sourceInventory.CountItem(itemType));
		else sourceCount = Math.Min(1, sourceInventory.CountItem(itemType));

		foreach (var inventory in to)
		{
			var targetCapacity = inventory.GetMaxTransferAmount(itemType);
			var amountToTransfer = Mathf.Min(sourceCount, targetCapacity);
			if (amountToTransfer <= 0) continue;
			Inventory.TransferItem(sourceInventory, inventory, itemType, amountToTransfer);
			break;
		}
	}
}
