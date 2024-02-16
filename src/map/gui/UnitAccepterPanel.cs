using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.gui;

public partial class UnitAccepterPanel : PanelContainer
{
	private Inventory _inventory;

	public UnitAccepterPanel()
	{
		CustomMinimumSize = new Vector2(0, 100);
	}
	
	public UnitAccepterPanel(Inventory inventory) : this()
	{
		_inventory = inventory;
	}

	
	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (@event is not InputEventMouseButton mouseButton  
		    || mouseButton.ButtonIndex != MouseButton.Left 
		    || !mouseButton.Pressed
		    || mouseButton.Canceled) return;

		
		GD.Print("UnitAccepterPanel clicked");

		var unitContainer = GetChildren().OfType<ScrollContainer>()
			.SelectMany(scroll => scroll.GetChildren().OfType<UnitsBox>())
			.FirstOrDefault();
		if (unitContainer == null) return;
		
		var units = MapGui.Instance.ArmySetup.GetAllSelected();

		foreach (var unit in units)
		{
			Inventory.TransferItem(unit.Inventory, unitContainer.GetInventory(), unit.UnitName, 1);
		}
		
		MapGlobals.UpdateListeners.ForEach(listener => listener());
	}
}
