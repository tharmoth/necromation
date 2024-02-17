using Godot;
using Necromation;
using Necromation.gui;

public partial class CrafterGUI : Control
{
	public void Display(ICrafter crafter)
	{
		Visible = true;
		
		GetNode<TransferInventory>("%Player").InventoryToDisplay = Globals.PlayerInventory;
		GetNode<TransferInventory>("%Player").InventoryToTransferTo = crafter.GetInputInventory();
		
		GetNode<TransferInventory>("%Input").InventoryToDisplay = crafter.GetInputInventory();
		GetNode<TransferInventory>("%Input").InventoryToTransferTo = Globals.PlayerInventory;
		
		GetNode<TransferInventory>("%Output").InventoryToDisplay = crafter.GetOutputInventory();
		GetNode<TransferInventory>("%Output").InventoryToTransferTo = Globals.PlayerInventory;
		
		GetNode<ChangeRecipeButton>("%ChangeRecipeButton")._crafter = crafter;
		
		GetNode<Label>("%Label").Text = crafter.GetRecipe()?.Name ?? "No Recipe";

		if (crafter is Node progress)
		{
			GetNode<ProgressTracker>("%ProgressBar").NodeToTrack = progress;
		}
	}
}
