using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class ArmySetup : PanelContainer
{
	private Container CommanderList => GetNode<Container>("%CommanderList");
	
	private Province _province;
	
	private Label ProvenceNameLabel => GetNode<Label>("%ProvinceNameLabel");

	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	private Control ProvinceUnitBox => GetNode<Control>("%ProvinceUnitBox");
	
	public static ArmySetup Display(Province province)
	{
		var gui = GD.Load<PackedScene>("res://src/map/gui/ArmySetup/army_setup.tscn").Instantiate<ArmySetup>();
		gui._province = province;
		MapGui.Instance.AddChild(gui);
		return gui;
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		_province.Units.Listeners.Remove(UpdateProvince);
	}
	
	public override void _Ready()
	{
		base._Ready();
        
		UpdateCommanders();
		UpdateProvince();
		SetupProvince();
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
	}

	private void SetupProvince()
	{
		_province.Units.Listeners.Add(UpdateProvince);
		
		var parent = ProvinceUnitBox.GetParent();
		ProvinceUnitBox.QueueFree();
		
		var squad = GD.Load<PackedScene>("res://src/map/gui/ArmySetup/army_setup_squad.tscn").Instantiate<ArmySetupSquad>();
		squad.Init(_province.Units, false);
		parent.AddChild(squad);
	}
	
}
