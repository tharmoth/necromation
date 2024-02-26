using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.gui;

public partial class ArmySetupSquad : PanelContainer
{
	private Container UnitList => GetNode<Container>("%UnitList");
	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	private Control UnitsBox => GetNode<Control>("%UnitsBox");
	private Control InfoPanel => GetNode<Control>("%InfoPanel");
	
	private Inventory Squad = new();

	public void Init(Inventory squad, bool showInfoPanel = true)
	{
		Squad = squad;
		InfoPanel.Visible = showInfoPanel;
	}

	public override void _Ready()
	{
		base._Ready();
		// Get rid of the mockup units
		UnitList.GetChildren().ToList().ForEach(child => child.QueueFree());
		
		Update();
		Squad.Listeners.Add(Update);
		UnitsBox.GuiInput += @event =>
		{
			if (@event is not InputEventMouseButton mouseButton) return;
			if (mouseButton.ButtonIndex != MouseButton.Left || !mouseButton.Pressed) return;
			GD.Print("clickyboi");

			var units = GetTree().GetNodesInGroup("SquadUnit")
				.OfType<SquadUnit>()
				.Where(unit => unit.Selected)
				.ToList();
			
			// I should do this in batches but if I want to make units unique instances I will need to do it this way in the future.
			foreach (var unit in units)
			{
				Inventory.TransferItem(unit.Inventory, Squad, unit.UnitName);
			}
			
			units.ForEach(unit => unit.SetSelected(false));
		};
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Squad.Listeners.Remove(Update);
	}

	private void AddMissingUnits()
	{
		foreach (var (item, count) in Squad.Items)
		{
			var units = UnitList.GetChildren().OfType<SquadUnit>().Where(box => box.UnitName == item).ToList();
			if (units.Count >= count) continue;
			for (int i = 0; i < count - units.Count; i++)
			{
				AddUnit(item);
			}
		}
	}

	private void RemoveExtraUnits()
	{
		// Count how many of each type of unit is in the squad
		var unitTypes =  UnitList.GetChildren().OfType<SquadUnit>().Select(unit => unit.UnitName).ToHashSet();
		foreach (var item in unitTypes)
		{
			var count = Squad.CountItem(item);
			var units = UnitList.GetChildren().OfType<SquadUnit>().Where(box => box.UnitName == item).ToList();
			if (units.Count <= count) continue;
			units.GetRange(count, units.Count - count).ForEach(unit => unit.QueueFree());
		}
	}

	public void Update()
	{
		UnitCountLabel.Text = Squad.CountAllItems().ToString() + " Units";
		AddMissingUnits();
		RemoveExtraUnits();
	}

	private void AddUnit(String name)
	{
		var squadUnit = new SquadUnit(name, Squad);
		squadUnit.Texture = Globals.Database.GetTexture(name);
		squadUnit.Listeners.Add(() => UnitList.GetChildren().OfType<SquadUnit>()
			.Where(unit => unit.UnitName == squadUnit.UnitName)
			.ToList()
			.ForEach(unit => unit.SetSelected(squadUnit.Selected)));
		UnitList.AddChild(squadUnit);
	}
	
}
