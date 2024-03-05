using Godot;
using Necromation;
using Necromation.gui;

public partial class CrafterGUI : Control
{
	public void Display(Inventory to, ICrafter crafter)
	{
		Visible = true;
		
		GetNode<TransferInventory>("%Player").InventoryToDisplay = to;
		GetNode<TransferInventory>("%Player").InventoryToTransferTo = crafter.GetInputInventory();
		
		GetNode<TransferInventory>("%Input").InventoryToDisplay = crafter.GetInputInventory();
		GetNode<TransferInventory>("%Input").InventoryToTransferTo = to;
		
		GetNode<TransferInventory>("%Output").InventoryToDisplay = crafter.GetOutputInventory();
		GetNode<TransferInventory>("%Output").InventoryToTransferTo = to;
		
		GetNode<ChangeRecipeButton>("%ChangeRecipeButton").Crafter = crafter;
		
		GetNode<Label>("%Label").Text = crafter.GetRecipe()?.Name ?? "No Recipe";

		// if (crafter is ProgressTracker.IProgress progress)
		// {
		// 	GetNode<ProgressTracker>("%ProgressBar").NodeToTrack = progress;
		// }
	}
}
