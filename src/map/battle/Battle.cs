using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class Battle : Node2D
{
	public const double TimeStep = .1;
	
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
