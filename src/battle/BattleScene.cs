using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.battle;
using Necromation.bridges;
using Necromation.map;
using Necromation.map.character;

public partial class BattleScene : Scene
{
	public const double NormalTimeStep = .5;
	public const double FastTimeStep = .25;
	public const double SonicTimeStep = .1;
	public static double TimeStep = .5;
	private BattleGUI _battleGui;
	public override BattleGUI Gui => _battleGui;
	public BattleTileMap TileMap;

	private bool _complete = false;

	public override void _Ready()
	{
		base._Ready();
		MusicManager.PlayBattleMusic();
		TileMap = GetNode<BattleTileMap>("%TileMap");
		_battleGui = GetNode<BattleGUI>("%GUI");

		var commanders = MapBattleBridge.Commanders;
		foreach(var commander in commanders)
		{
			var spawner = new UnitSpawner(commander, commander.Team);
			Vector2I spawnLocation;
			
			// (0, 0) = BattleTileMap.X / 10, BattleTileMap.Y / 10
			// (100, 100) = BattleTileMap.X * 4 / 10, BattleTileMap.Y * 9 / 10
			var commanderOffset = commander.SpawnLocation - Vector2I.One * 50;
			var commanderOffsetScaled = new Vector2I(
				Mathf.RoundToInt(BattleTileMap.X * commanderOffset.X / 100.0f * 4 / 10.0f),
				Mathf.RoundToInt(BattleTileMap.Y * commanderOffset.Y / 100.0f * 6 / 10.0f)
			);

			if (commander.Team == "Player")
			{
				spawnLocation = new Vector2I(BattleTileMap.X / 4, BattleTileMap.Y / 2);
				spawnLocation += commanderOffsetScaled;
			}
			else
			{
				spawnLocation = new Vector2I(BattleTileMap.X * 3 / 4, BattleTileMap.Y / 2);
				spawnLocation += new Vector2I(-commanderOffsetScaled.X, commanderOffsetScaled.Y);
			}
			
			spawner.GlobalPosition = Globals.BattleScene.TileMap.MapToGlobal(spawnLocation);
			AddChild(spawner);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		var playerUnitCount = Globals.UnitManager.GetGroup("Player").Count;
		var enemyUnitCount = Globals.UnitManager.GetGroup("Enemy").Count;

		if ((playerUnitCount != 0 && enemyUnitCount != 0) || _complete) return;
		
		_complete = true;
		var winner = playerUnitCount == 0 ? "Enemy" : "Player";
		MapBattleBridge.EndBattle(winner);
	}

	public override void OnOpen()
	{
		Camera.GlobalPosition = new Vector2(BattleTileMap.X / 2.0f, BattleTileMap.Y / 2.0f) * BattleTileMap.TileSize;
	}

	public override void OnClose()
	{
		// var team = Globals.BattleScene.TileMap.GetEntities(BattleTileMap.Unit)
		// 	.Select(unit => unit as Unit)
		// 	.Where(unit => unit != null)
		// 	.Select(unit => unit.Team)
		// 	.Distinct()
		// 	.First();
		//
		// // Globals.BattleScene.Provence.Owner = team;
		// foreach (var army in Globals.BattleArmies)
		// {
		// 	army.GetComponent<ArmyComponent>().Commanders.RemoveWhere(commander => commander.Units.CountItems() == 0);
		// }
		//
		// if (team == "Enemy")
		// {
		// 	Globals.MapScene.Actors
		// 		.Where(actor => actor.GetComponent<StatsComponent>().Team == "Player")
		// 		.ToList()
		// 		.ForEach(actor => actor.GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(MapTileMap.FactoryPosition));
		// }
		
		Globals.BattleScene.QueueFree();
		Globals.MapScene.UpdateListeners.ForEach(listener => listener());
	}
}
