using Godot;
using System;

public partial class ContainerGUI : PanelContainer
{
	public void Display(Inventory playerInventory, Inventory containerInventory, string title = "Container")
	{
		Visible = true;

		GetNode<TransferInventory>("%PlayerInventory").InventoryToDisplay = playerInventory;
		GetNode<TransferInventory>("%PlayerInventory").InventoryToTransferTo = containerInventory;

		GetNode<TransferInventory>("%ContainerInventory").InventoryToDisplay = containerInventory;
		GetNode<TransferInventory>("%ContainerInventory").InventoryToTransferTo = playerInventory;
		
		GetNode<Label>("%Label").Text = title;
	}
}
