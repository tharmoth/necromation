using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class ArmyLayout : PanelContainer
{
	private Container CommanderList => GetNode<Container>("%CommanderList");
	
	private Province _province;
	
	private Label ProvenceNameLabel => GetNode<Label>("%ProvinceNameLabel");
	private Label ProvinceUnitCountLabel => GetNode<Label>("%ProvinceUnitCountLabel");
	
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
		
		ProvenceNameLabel.Text = _province.Name;
		ProvinceUnitCountLabel.Text = _province.Units.CountAllItems().ToString();
		
		UpdateCommanders();
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
		setupCommander.Commander = commander;
		CommanderList.AddChild(setupCommander);
	}
}
