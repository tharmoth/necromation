using Godot;
using System;
using System.Collections.Generic;
using Necromation.map;
using Necromation.map.gui;

public partial class ArmySetup : PanelContainer
{
	public override void _Ready()
	{
		MapGlobals.UpdateListeners.Add(Update);
		VisibilityChanged += () =>
		{
			if (IsVisibleInTree())
			{
				Update();
			}
		};
	}

	public List<UnitRect> GetAllSelected()
	{
		var units = GetNode<UnitsBox>("%UnitsBox").GetAllSelected();
		units.AddRange(GetNode<CommanderUnitList>("%CommanderUnitList").GetAllSelected());
		return units;
	}
	
	private void Update()
	{
		GetNode<CommanderUnitList>("%CommanderUnitList").Update();
		GetNode<UnitsBox>("%UnitsBox").Update(MapGlobals.SelectedProvince.GetOutputInventory());
	}
}
