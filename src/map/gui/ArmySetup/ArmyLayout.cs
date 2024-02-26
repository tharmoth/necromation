using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class ArmyLayout : PanelContainer
{
	private Container CommanderList => GetNode<Container>("%CommanderList");
	
	private Province _province;
	
	private Label ProvenceNameLabel => GetNode<Label>("%ProvinceNameLabel");
	
	private Container UnitList => GetNode<Container>("%UnitList");
	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	private Control UnitsBox => GetNode<Control>("%UnitsBox");
	
	public List<SquadUnit> SelectedUnits = new();
	
	public static ArmyLayout Display(Province province)
	{
		var gui = GD.Load<PackedScene>("res://src/map/gui/ArmySetup/army_setup.tscn").Instantiate<ArmyLayout>();
		gui._province = province;
		MapGui.Instance.AddChild(gui);
		return gui;
	}
	
	public override void _Ready()
	{
		base._Ready();
		
		UpdateCommanders();
		UpdateProvince();
		
		UnitsBox.GuiInput += @event =>
		{
			if (@event is not InputEventMouseButton mouseButton) return;
			if (mouseButton.ButtonIndex != MouseButton.Left || !mouseButton.Pressed) return;
			GD.Print("clickyboi");

			var units = GetTree().GetNodesInGroup("SquadUnit")
				.OfType<SquadUnit>()
				.Where(unit => unit.Selected)
				.ToList();
			
			// Get a list how how many of each unit is selected
			var test = units.GroupBy(unit => unit.UnitName).ToList();
			test.ForEach(group => GD.Print(group.Key + ": " + group.Count()));
			foreach (var unit in units)
			{
				Inventory.TransferItem(unit.Inventory, _province.Units, unit.UnitName);
			}
		};
	}

	private void UpdateCommanders()
	{
		CommanderList.GetChildren().ToList().ForEach(child => child.QueueFree());
		_province.Commanders
			.OrderBy(recipe => recipe.Name)
			.ToList().ForEach(AddCommander);
	}
	
	private void AddCommander(Commander commander)
	{
		var setupCommander = GD.Load<PackedScene>("res://src/map/gui/ArmySetup/army_setup_commander.tscn").Instantiate<ArmySetupCommander>();
		setupCommander.Init(commander);
		CommanderList.AddChild(setupCommander);
	}
	
	private void UpdateProvince()
	{
		ProvenceNameLabel.Text = _province.Name + ",";
		UnitCountLabel.Text = _province.Units.CountAllItems().ToString();

		UnitList.GetChildren().ToList().ForEach(child => child.QueueFree());
		foreach (var (name, count) in _province.Units.Items)
		{
			for (int i = 0; i < count; i++)
			{
				var squadUnit = new SquadUnit(name, _province.Units);
				squadUnit.Texture = Globals.Database.GetTexture(name);
				squadUnit.Listeners.Add(() => UnitList.GetChildren().OfType<SquadUnit>()
					.Where(unit => unit.UnitName == squadUnit.UnitName)
					.ToList()
					.ForEach(unit => unit.SetSelected(squadUnit.Selected)));
				UnitList.AddChild(squadUnit);
			}
		}
	}
}
