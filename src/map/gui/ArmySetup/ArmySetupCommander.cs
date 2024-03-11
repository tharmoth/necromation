using Godot;
using System;
using System.Linq;
using Necromation.map;
using Necromation.map.character;

public partial class ArmySetupCommander : PanelContainer
{
	private Label NameLabel => GetNode<Label>("%NameLabel");
	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	private Container SquadList => GetNode<Container>("%SquadList");

	private Commander _commander;
	
	public void Init(Commander commander)
	{
		_commander = commander;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_commander.Units.Listeners.Remove(Update);
	}

	public override void _Ready()
	{
		base._Ready();
		_commander.Units.Listeners.Add(Update);
		UpdateSquads();
		Update();
	}

	private void Update()
	{
		NameLabel.Text = _commander.CommanderName;
		UnitCountLabel.Text = _commander.Units.CountItems() + "/" + _commander.CommandCap;
	}

	private void UpdateSquads()
	{
		SquadList.GetChildren().ToList().ForEach(child => child.QueueFree());
		AddSquad();
	}
	
	private void AddSquad()
	{
		var squad = GD.Load<PackedScene>("res://src/map/gui/ArmySetup/army_setup_squad.tscn").Instantiate<ArmySetupSquad>();
		squad.Init(_commander.Units, true, _commander);
		SquadList.AddChild(squad);
	}
}
