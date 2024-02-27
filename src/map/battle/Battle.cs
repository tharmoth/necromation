using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class Battle : Node2D
{
	public const double TimeStep = .5;
	
	public BattleTileMap TileMap;
	public CanvasLayer GUI;
	public Province Provence;
	
	public override void _EnterTree()
	{
		Globals.BattleScene = this;
	}

	public override void _Ready()
	{
		base._Ready();
		Globals.BattleCamera = GetNode<Camera2D>("Camera2D");
		
		if (Provence == null) Provence = new Province();
		
		if (Provence.Commanders.Count == 0)
		{
			var PlayerCommanderWarrior = new Commander(new Province(), "Player");
			PlayerCommanderWarrior.Units.Insert("Warrior", 100);
			Provence.Commanders.Add(PlayerCommanderWarrior);
			
			var EnemyCommanderWarrior = new Commander(new Province(), "Enemy");
			EnemyCommanderWarrior.Units.Insert("Warrior", 100);
			Provence.Commanders.Add(EnemyCommanderWarrior);
			
			var PlayerCommanderArcher = new Commander(new Province(), "Player");
			PlayerCommanderArcher.SpawnLocation = new Vector2I(0, 25);
			PlayerCommanderArcher.Units.Insert("Archer", 100);
			Provence.Commanders.Add(PlayerCommanderArcher);
			
			var EnemyCommanderArcher = new Commander(new Province(), "Enemy");
			EnemyCommanderArcher.SpawnLocation = new Vector2I(30, 25);
			EnemyCommanderArcher.Units.Insert("Archer", 100);
			Provence.Commanders.Add(EnemyCommanderArcher);
		}
		
		foreach(var commander in Provence.Commanders)
		{
			var spawner = new UnitSpawner(commander, commander.Team);
			
			if (commander.Team == "Player")
			{
				var spawnLocation = commander.SpawnLocation;
				spawnLocation = spawnLocation + new Vector2I(0, 25);
				spawner.GlobalPosition = Globals.BattleScene.TileMap.MapToGlobal(spawnLocation);
			}
			else
			{
				var spawnLocation = commander.SpawnLocation;
				spawnLocation = spawnLocation + new Vector2I(50, 25);
				spawner.GlobalPosition = Globals.BattleScene.TileMap.MapToGlobal(spawnLocation);
			}
			
			AddChild(spawner);
		}


	}
}
