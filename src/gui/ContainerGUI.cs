using Godot;
using System;

public partial class ContainerGUI : Control
{
	private Inventory _inventory;
	
	public void Display(Inventory inventory)
	{
		_inventory = inventory;
		Visible = true;
		
		GetNode<TransferInventory>("%Player").SourceInventory = Inventory.Instance;
		GetNode<TransferInventory>("%Player").TargetInventory = inventory;
		
		GetNode<TransferInventory>("%Container").SourceInventory = inventory;
		GetNode<TransferInventory>("%Container").TargetInventory = Inventory.Instance;
	}
}
