using Godot;
using System;
using System.Linq;
using Necromation.gui;

public partial class TransferInventory : VBoxContainer
{
	private Inventory _inventoryToDisplay;
	public Inventory InventoryToDisplay
	{
		get => _inventoryToDisplay;
		set
		{
			_inventoryToDisplay?.Listeners.Remove(Update);
			_inventoryToDisplay = value;
			_inventoryToDisplay.Listeners.Add(Update);
			Update();
		}
		
	}
	public Inventory InventoryToTransferTo { get; set; }

	private void Update()
	{
		RemoveMissingItems();
		AddButtons();
		GetChildren().OfType<ItemButton>().ToList().ForEach(child => child.Update());
	}
	
	private void RemoveMissingItems()
	{
		foreach (var label in GetChildren().OfType<ItemButton>().ToHashSet())
		{
			if (InventoryToDisplay.Items.ContainsKey(label.ItemType)) continue;
			
			label.QueueFree();
		}
	}

	private void AddButtons()
	{
		foreach (var item in InventoryToDisplay.Items.Keys)
		{
			if (GetChildren().OfType<ItemButton>().Any(label => label.ItemType == item)) continue;

			var button = new ItemButton(item, InventoryToDisplay, InventoryToTransferTo);
			AddChild(button);
			button.Update();
			
			button.Pressed += () =>
			{
				if (!InventoryToDisplay.Items.ContainsKey(button.ItemType)) return;
				InventoryToDisplay.RemoveItem(button.ItemType);
				InventoryToTransferTo.AddItem(button.ItemType);
			};
		}
	}
	
	private partial class ItemButton : Button
	{
		public string ItemType { get;}
		private Inventory SourceInventory { get; }
		private Inventory TargetInventory { get; }
		
		public ItemButton(string itemType, Inventory sourceInventory, Inventory targetInventory)
		{
			this.ItemType = itemType;
			this.SourceInventory = sourceInventory;
			this.TargetInventory = targetInventory;
		}
		
		public void Update()
		{
			if (SourceInventory.Items.ContainsKey(ItemType))
			{
				Text = $"{ItemType} x{SourceInventory.Items[ItemType]}";
			}
		}
	}
}
