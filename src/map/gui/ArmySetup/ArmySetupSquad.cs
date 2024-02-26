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
	
	private Inventory Squad = new();

	public void Init(Inventory squad)
	{
		Squad = squad;
	}

	public override void _Ready()
	{
		base._Ready();

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
			
			foreach (var unit in units)
			{
				Inventory.TransferItem(unit.Inventory, Squad, unit.UnitName);
			}
		};
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Squad.Listeners.Remove(Update);
	}

	private void RemoveMissingUnits()
	{

		
	}

	public void Update()
	{
		UnitCountLabel.Text = Squad.CountAllItems().ToString() + " Units";
		
		UnitList.GetChildren().ToList().ForEach(child => child.QueueFree());
		foreach (var (name, count) in Squad.Items)
		{
			for (int i = 0; i < count; i++)
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
