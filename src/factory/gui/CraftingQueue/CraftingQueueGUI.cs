using Godot;
using System;
using Necromation;

public partial class CraftingQueueGui : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Globals.FactoryScene.CraftingQueue.Listeners.Add(UpdateCraftingQueue);
		UpdateCraftingQueue();
	}

	private void UpdateCraftingQueue()
	{
		CraftingQueueItemBox.Update(Globals.FactoryScene.CraftingQueue.Queue, this);
	}
}
