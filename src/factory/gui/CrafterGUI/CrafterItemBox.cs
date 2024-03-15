using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class CrafterItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/factory/gui/CrafterGUI/CrafterItemBox.tscn");
	private static readonly ShaderMaterial GreyScale = GD.Load<ShaderMaterial>("res://src/factory/shaders/greyscale.tres");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private List<Inventory> _to;
	private Inventory _sourceInventory;
	private int _count;
	private bool _isInput;
	
	
	// Static Accessor
	public static void UpdateInventory(Inventory from, List<Inventory> to, Container list, Recipe recipe, bool isInput)
	{
		AddMissing(from, to, list, recipe, isInput);
		RemoveExtra(from, list);
	}
	
	// Static Accessor
	private static void AddInventoryItem(string item, int count, Inventory from, List<Inventory> to, Container list, bool isInput)
	{
		var inventoryItem = ItemScene.Instantiate<CrafterItemBox>();
		inventoryItem.Init(from, to, item, count, isInput);
		list.AddChild(inventoryItem);
	}
	
	private static void AddMissing(Inventory from, List<Inventory> to, Container list, Recipe recipe, bool isInput)
	{
		foreach (var (item, count) in isInput ? recipe.Ingredients : recipe.Products)
		{
			var itemBox = list.GetChildren().OfType<CrafterItemBox>().FirstOrDefault(box => box.ItemType == item);
			if (itemBox != null) continue;
			AddInventoryItem(item, count, from, to, list, isInput);
		}
	}

	private static void RemoveExtra(Inventory from, Container list)
	{
		list.GetChildren().OfType<InventoryItem>()
			.Where(inventoryItem => from.CountItem(inventoryItem.ItemType) == 0)
			.ToList()
			.ForEach(item => item.QueueFree());
	}
	
	private void Init(Inventory source, List<Inventory> to, string item, int count, bool isInput)
	{
		ItemType = item;
		_count = count;
		_isInput = isInput;
		CountLabel.Text = source.CountItem(item).ToString();
		_to = to;
		_sourceInventory = source;
		source.Listeners.Add(UpdateCount);
		UpdateCount();
		// a63c4560
		// b1832660
		if (!_isInput) ColorRect.Color = new Color("b1832660");
		
		Button.Pressed += () =>
		{
			if (_to == null || _sourceInventory == null)
			{
				Globals.Player.Selected = ItemType;
				return;
			}
		
			var sourceCount = Math.Min(1, _sourceInventory.CountItem(ItemType));
			if (Input.IsActionPressed("shift"))
			{
				sourceCount = _sourceInventory.CountItem(ItemType);
			} else if (Input.IsActionPressed("control"))
			{
				sourceCount = Math.Min(5, _sourceInventory.CountItem(ItemType));
			}
		
			foreach (var inventory in _to)
			{
				var targetCapacity = inventory.GetMaxTransferAmount(ItemType);
				var amountToTransfer = Mathf.Min(sourceCount, targetCapacity);
				if (amountToTransfer <= 0) continue;
				Inventory.TransferItem(_sourceInventory, inventory, ItemType, amountToTransfer);
				break;
			}
		};
	}

	private void UpdateCount()
	{
		var count = _sourceInventory.CountItem(ItemType);
		CountLabel.Text = count.ToString();
		CountLabel.Visible = count > 0;
		ColorRect.Visible = count < _count && _isInput || count > 0 && !_isInput;
		
		Icon.Material = count > 0 ? null : GreyScale;
		Icon.Modulate = count > 0 ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.75f, 0.75f, 0.75f);
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		_sourceInventory.Listeners.Remove(UpdateCount);
	}
}
