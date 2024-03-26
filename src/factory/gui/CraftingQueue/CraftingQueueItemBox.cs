using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class CraftingQueueItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene ItemScene = GD.Load<PackedScene>("res://src/factory/gui/CraftingQueue/CraftingQueueItemBox.tscn");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private CraftingQueue.CraftingQueueItem _item;

	
	// Static Accessor
	public static void Update(ImmutableList<CraftingQueue.CraftingQueueItem> queue, Container list)
	{
		AddMissing(queue, list);
		RemoveExtra(queue, list);
	}
	
	// Static Accessor
	private static void AddItem(CraftingQueue.CraftingQueueItem item, Container list)
	{
		var itemBox = ItemScene.Instantiate<CraftingQueueItemBox>();
		itemBox.Init(item);
		list.AddChild(itemBox);
	}
	
	private static void AddMissing(ImmutableList<CraftingQueue.CraftingQueueItem> queue, Container list)
	{
		foreach (var item in queue)
		{
			var itemBox = list.GetChildren().OfType<CraftingQueueItemBox>().FirstOrDefault(itemBox => itemBox._item == item);
			if (itemBox != null) continue;
			AddItem(item, list);
		}
	}

	private static void RemoveExtra(ImmutableList<CraftingQueue.CraftingQueueItem> queue, Container list)
	{
		list.GetChildren().OfType<CraftingQueueItemBox>()
			.Where(inventoryItem => !queue.Contains(inventoryItem._item))
			.ToList()
			.ForEach(item => item.QueueFree());
	}

	private void Init(CraftingQueue.CraftingQueueItem item)
	{
		_item = item;
		ItemType = item.Recipe.Products.First().Key;
		CountLabel.Text = item.Count.ToString();
		ProgressBar.Visible = true;
		ProgressBar.Value = 0;
		
		Button.GuiInput += @event =>
		{
			switch (@event)
			{
				case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }:
				{
					Globals.FactoryScene.CraftingQueue.CancelItem(item, 1);
					Globals.FactoryScene.CraftingQueue.Listeners.ForEach(listener => listener());
					CountLabel.Text = _item.Count.ToString();
					break;
				}
				case InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true }:
					Globals.FactoryScene.CraftingQueue.CancelItem(item, item.Count);
					Globals.FactoryScene.CraftingQueue.Listeners.ForEach(listener => listener());
					CountLabel.Text = _item.Count.ToString();
					break;
			}
		};
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		// Polling is bad scott.
		if (Globals.FactoryScene.CraftingQueue.Queue.IndexOf(_item) != 0) return;
		
		ProgressBar.Value = (float) (Globals.FactoryScene.CraftingQueue.Time / CraftingQueue.TimePerCraft) * 100;
		CountLabel.Text = _item.Count.ToString();
	}
}
