using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class InventoryItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/factory/gui/InventoryGui/InventoryItemBox.tscn");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private List<Inventory> _to;
	private Inventory _sourceInventory;
	private string _cachedSelected = "";
	
	// Static Accessor
	public static void UpdateInventory(Inventory from, List<Inventory> to, Container list)
	{
		AddMissing(from, to, list);
		RemoveExtra(from, list);
		var sorted = list.GetChildren().OfType<InventoryItemBox>()
			.ToList();
		sorted.Sort((a, b) 
			=> Database.Instance.CompareItems(a.ItemType, b.ItemType));
		sorted.ForEach(item => list.MoveChild(item, sorted.IndexOf(item)));
	}
	
	// Static Accessor
	private static void AddInventoryItem(string item, Inventory from, List<Inventory> to, Container list)
	{
		var inventoryItem = ItemScene.Instantiate<InventoryItemBox>();
		inventoryItem.Init(from, to, item);
		list.AddChild(inventoryItem);
	}
	
	private static void AddMissing(Inventory from, List<Inventory> to, Container list)
	{
		foreach (var (item, count) in from.Items)
		{
			var itemBox = list.GetChildren().OfType<InventoryItemBox>().FirstOrDefault(box => box.ItemType == item);
			if (itemBox != null) continue;
			AddInventoryItem(item, from, to, list);
		}
	}

	private static void RemoveExtra(Inventory from, Container list)
	{
		list.GetChildren().OfType<InventoryItemBox>()
			.Where(inventoryItem => from.CountItem(inventoryItem.ItemType) == 0)
			.ToList()
			.ForEach(item => item.Free());
	}
	
	private void Init(Inventory source, List<Inventory> to, string item)
	{
		ItemType = item;
		CountLabel.Text = source.CountItem(item).ToString();
		_to = to;
		_sourceInventory = source;
		source.Listeners.Add(UpdateCount);

		Button.Pressed += ButtonPress;
	}

	/// <summary>
	/// When an inventory button is pressed, do one of two things:<br />
	/// 1. If shift or control is pressed try to transfer items to the target directly.<br />
	/// 2. If no shift or control is pressed or there is no valid target, select the item for the player to use.
	/// </summary>
	private void ButtonPress()
	{
		var sourceCount = 0;
		if (Input.IsActionPressed("shift"))
		{
			sourceCount = _sourceInventory.CountItem(ItemType);
		} 
		else if (Input.IsActionPressed("control"))
		{
			sourceCount = Math.Min(5, _sourceInventory.CountItem(ItemType));
		}
			
		if (_to == null || _sourceInventory == null || sourceCount == 0)
		{
			Globals.Player.Selected = Globals.Player.Selected == ItemType ? null : ItemType;
			return;
		}

		foreach (var inventory in _to)
		{
			var targetCapacity = inventory.GetMaxTransferAmount(ItemType);
			var amountToTransfer = Mathf.Min(sourceCount, targetCapacity);
			if (amountToTransfer <= 0) continue;
			Inventory.TransferItem(_sourceInventory, inventory, ItemType, amountToTransfer);
			break;
		}
	}

	private void UpdateCount()
	{
		CountLabel.Text = _sourceInventory.CountItem(ItemType).ToString();
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

	public override void _ExitTree()
	{
		base._ExitTree();
		_sourceInventory.Listeners.Remove(UpdateCount);
	}
}
