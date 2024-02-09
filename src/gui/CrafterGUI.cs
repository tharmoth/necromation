using Godot;
using System;
using Necromation.gui;

public partial class CrafterGUI : Control
{
	public void Display(ICrafter crafter)
	{
		Visible = true;
		
		GetNode<TransferInventory>("%Player").InventoryToDisplay = Inventory.Instance;
		GetNode<TransferInventory>("%Player").InventoryToTransferTo = crafter.GetInputInventory();
		
		GetNode<TransferInventory>("%Input").InventoryToDisplay = crafter.GetInputInventory();
		GetNode<TransferInventory>("%Input").InventoryToTransferTo = Inventory.Instance;
		
		GetNode<TransferInventory>("%Output").InventoryToDisplay = crafter.GetOutputInventory();
		GetNode<TransferInventory>("%Output").InventoryToTransferTo = Inventory.Instance;
		
		GetNode<ChangeRecipeButton>("%ChangeRecipeButton")._crafter = crafter;
		
		GetNode<Label>("%Label").Text = crafter.GetRecipe(null)?.Name ?? "No Recipe";

		if (crafter is Node progress)
		{
			GetNode<ProgressTracker>("%ProgressBar").NodeToTrack = progress;
		}
		
	}
}
