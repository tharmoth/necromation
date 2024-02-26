using Godot;
using System;
using System.Linq;
using Necromation.map;
using Necromation.map.character;

public partial class ArmySetupCommander : PanelContainer
{
	private Label NameLabel => GetNode<Label>("%NameLabel");
	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	private Label SquadCountLabel => GetNode<Label>("%SquadCountLabel");
	private Container SquadList => GetNode<Container>("%SquadList");

	private Commander _commander = new(new Province(), "Player");
	
	public void Init(Commander commander)
	{
		_commander = commander;
	}
	
	public override void _Ready()
	{
		base._Ready();
		Update();
	}

	private void Update()
	{
		NameLabel.Text = _commander.Name;
		UnitCountLabel.Text = _commander.Units.CountAllItems() + "/" + _commander.CommandCap;
		SquadCountLabel.Text = "1/" + _commander.SquadCap;

		UpdateSquads();
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
