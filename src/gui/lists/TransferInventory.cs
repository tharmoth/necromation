using Godot;
using System;
using System.Linq;
using Necromation.gui;

public partial class TransferInventory : VBoxContainer
{
	private Inventory _sourceInventory;
	public Inventory SourceInventory
	{
		get => _sourceInventory;
		set
		{
			_sourceInventory?.Listeners.Remove(Update);
			_sourceInventory = value;
			_sourceInventory.Listeners.Add(Update);
			Update();
		}
		
	}
	public Inventory TargetInventory { get; set; }

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
			if (SourceInventory.Items.ContainsKey(label.ItemType)) continue;
			
			label.QueueFree();
		}
	}

	private void AddButtons()
	{
		foreach (var item in SourceInventory.Items.Keys)
		{
			if (GetChildren().OfType<ItemButton>().Any(label => label.ItemType == item)) continue;

			var button = new ItemButton(item, SourceInventory, TargetInventory);
			AddChild(button);
			button.Update();
			
			button.Pressed += () =>
			{
				if (!SourceInventory.Items.ContainsKey(button.ItemType)) return;
				SourceInventory.RemoveItem(button.ItemType);
				TargetInventory.AddItem(button.ItemType);
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
