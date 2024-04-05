using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.battle;
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

	public Province Provence;

	public override void _Ready()
	{
		base._Ready();
		MusicManager.PlayBattleMusic();
		TileMap = GetNode<BattleTileMap>("%TileMap");
		_battleGui = GetNode<BattleGUI>("%GUI");
			
		if (Provence == null) Provence = new Province("");
		
		if (Provence.Commanders.Count == 0)
		{
			
			// var playerCavNorth = new Commander(Provence, "Player");
			// playerCavNorth.CurrentCommand = Commander.Command.HoldAndAttack;
			// playerCavNorth.TargetType = Commander.Target.Archers;
			// playerCavNorth.SpawnLocation = new Vector2I(100, 0);
			// playerCavNorth.Units.Insert("Gravebound Charger", 25);
			//
			// var playerCavSouth = new Commander(Provence, "Player");
			// playerCavSouth.CurrentCommand = Commander.Command.HoldAndAttack;
			// playerCavSouth.TargetType = Commander.Target.Archers;
			// playerCavSouth.SpawnLocation = new Vector2I(100, 100);
			// playerCavSouth.Units.Insert("Gravebound Charger", 25);
			
			var playerInfantry = new Commander(Provence, "Player");
			// playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			playerInfantry.TargetType = Commander.Target.Random;
			playerInfantry.SpawnLocation = new Vector2I(50, 50);
			playerInfantry.Units.Insert("Deathbound Mage", 16);
			
			var playerInfantry2 = new Commander(Provence, "Player");
			// playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			playerInfantry2.TargetType = Commander.Target.Closest;
			playerInfantry2.SpawnLocation = new Vector2I(100, 25);
			playerInfantry2.Units.Insert("Dreadplate Phalanx", 200);
			
			var playerInfantry3 = new Commander(Provence, "Player");
			// playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			playerInfantry3.TargetType = Commander.Target.Closest;
			playerInfantry3.SpawnLocation = new Vector2I(100, 75);
			playerInfantry3.Units.Insert("Dreadplate Phalanx", 200);
			//
			// var playerInfantry4 = new Commander(Provence, "Player");
			// // playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			// playerInfantry4.TargetType = Commander.Target.Closest;
			// playerInfantry4.SpawnLocation = new Vector2I(100, 50);
			// playerInfantry4.Units.Insert("Awoken Skeleton", 200);
			//
			// var playerInfantry5 = new Commander(Provence, "Player");
			// // playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			// playerInfantry5.TargetType = Commander.Target.Closest;
			// playerInfantry5.SpawnLocation = new Vector2I(100, 50);
			// playerInfantry5.Units.Insert("Awoken Skeleton", 200);
			
			var playerArchers = new Commander(Provence, "Player");
			// playerArchers.CurrentCommand = Commander.Command.HoldAndAttack;
			playerArchers.TargetType = Commander.Target.Random;
			playerArchers.SpawnLocation = new Vector2I(75, 50);
			playerArchers.Units.Insert("Skeleton Marksman", 100);
			
			var enemyInfantry = new Commander(Provence, "Enemy");
			enemyInfantry.SpawnLocation = new Vector2I(100, 50);
			enemyInfantry.Units.Insert("Infantry", 200);
			
			var enemyInfantry2 = new Commander(Provence, "Enemy");
			enemyInfantry2.SpawnLocation = new Vector2I(100, 75);
			enemyInfantry2.Units.Insert("Infantry", 200);
			
			var enemyInfantry3 = new Commander(Provence, "Enemy");
			enemyInfantry3.SpawnLocation = new Vector2I(100, 25);
			enemyInfantry3.Units.Insert("Infantry", 200);
			
			var enemyInfantry4 = new Commander(Provence, "Enemy");
			enemyInfantry4.SpawnLocation = new Vector2I(100, 100);
			enemyInfantry4.Units.Insert("Infantry", 200);
						
			var enemyInfantry5 = new Commander(Provence, "Enemy");
			enemyInfantry5.SpawnLocation = new Vector2I(100, 0);
			enemyInfantry5.Units.Insert("Infantry", 200);
			
			var enemyInfantry6 = new Commander(Provence, "Enemy");
			enemyInfantry6.SpawnLocation = new Vector2I(50, 75);
			enemyInfantry6.Units.Insert("Infantry", 200);
			
			var enemyInfantry7 = new Commander(Provence, "Enemy");
			enemyInfantry7.SpawnLocation = new Vector2I(50, 25);
			enemyInfantry7.Units.Insert("Infantry", 200);
			
			var enemyArchers = new Commander(Provence, "Enemy");
			enemyArchers.TargetType = Commander.Target.Random;
			enemyArchers.SpawnLocation = new Vector2I(75, 50);
			enemyArchers.Units.Insert("Archer", 200);
		}
		
		foreach(var commander in Provence.Commanders)
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

	public override void OnOpen()
	{
		Provence = Globals.MapScene.SelectedProvince;
		Camera.GlobalPosition = new Vector2(BattleTileMap.X / 2.0f, BattleTileMap.Y / 2.0f) * BattleTileMap.TileSize;
	}

	public override void OnClose()
	{
		var team = Globals.BattleScene.TileMap.GetEntities(BattleTileMap.Unit)
			.Select(unit => unit as Unit)
			.Where(unit => unit != null)
			.Select(unit => unit.Team)
			.Distinct()
			.First();

		Globals.BattleScene.Provence.Owner = team;
		Globals.BattleScene.Provence.Commanders
			.Where(commander => commander.Team != team)
			.ToList()
			.ForEach(commander => commander.Kill());;
		Globals.BattleScene.QueueFree();
		Globals.MapScene.UpdateListeners.ForEach(listener => listener());
	}
}
