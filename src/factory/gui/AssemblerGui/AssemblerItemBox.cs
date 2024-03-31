using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class AssemblerItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/factory/gui/AssemblerGui/AssemblerItemBox.tscn");
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
		RemoveExtra(from, list, recipe, isInput);
	}
	
	// Static Accessor
	public static void AddInventoryItem(string item, int count, Inventory from, List<Inventory> to, Container list, bool isInput)
	{
		var inventoryItem = ItemScene.Instantiate<AssemblerItemBox>();
		inventoryItem.Init(from, to, item, count, isInput);
		list.AddChild(inventoryItem);
	}
	
	private static void AddMissing(Inventory from, List<Inventory> to, Container list, Recipe recipe, bool isInput)
	{
		if (recipe == null) return;
		foreach (var (item, count) in isInput ? recipe.Ingredients : recipe.Products)
		{
			var itemBox = list.GetChildren().OfType<AssemblerItemBox>().FirstOrDefault(box => box.ItemType == item);
			if (itemBox != null) continue;
			AddInventoryItem(item, count, from, to, list, isInput);
		}
	}

	private static void RemoveExtra(Inventory from, Container list, Recipe recipe, bool isInput)
	{
		if (recipe == null) return;
		var items = isInput ? recipe.Ingredients : recipe.Products;
		
		list.GetChildren().OfType<AssemblerItemBox>()
			.Where(inventoryItem => inventoryItem.ItemType == null || !items.ContainsKey(inventoryItem.ItemType))
			.ToList()
			.ForEach(item => item.QueueFree());
	}
	
	private void Init(Inventory source, List<Inventory> to, string item, int count, bool isInput)
	{
		_sourceInventory = source;
		_to = to;
		ItemType = item;
		_count = count;
		_isInput = isInput;
		
		_sourceInventory.Listeners.Add(UpdateCount);
		UpdateCount();
		// a63c4560
		// b1832660
		if (!_isInput) ColorRect.Color = new Color("b1832660");
		
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
		if (ItemType == selected) TransferToPlayer(_sourceInventory, selected);
		else TransferToSource(_sourceInventory, _to, ItemType);
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
		_sourceInventory?.Listeners.Remove(UpdateCount);
	}
}
