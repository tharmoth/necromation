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
	public const double TimeStep = .5;
	private BattleGUI _battleGui => GetNode<BattleGUI>("%GUI");
	public override BattleGUI Gui => _battleGui;
	public BattleTileMap TileMap;

	public Province Provence;

	public override void _Ready()
	{
		base._Ready();
		MusicManager.PlayBattleMusic();
		TileMap = GetNode<BattleTileMap>("%TileMap");
			
		if (Provence == null) Provence = new Province(Vector2I.Zero);
		
		if (Provence.Commanders.Count == 0)
		{
			// var PlayerCommanderInfantry = new Commander(Provence, "Player");
			// PlayerCommanderInfantry.SpawnLocation = new Vector2I(0, 0);
			// PlayerCommanderInfantry.Units.Insert("Gravebreaker", 500);
			
			var playerCavNorth = new Commander(Provence, "Player");
			playerCavNorth.CurrentCommand = Commander.Command.HoldAndAttack;
			playerCavNorth.TargetType = Commander.Target.Archers;
			playerCavNorth.SpawnLocation = new Vector2I(100, 0);
			playerCavNorth.Units.Insert("Gravebound Charger", 25);
			
			var playerCavSouth = new Commander(Provence, "Player");
			playerCavSouth.CurrentCommand = Commander.Command.HoldAndAttack;
			playerCavSouth.TargetType = Commander.Target.Archers;
			playerCavSouth.SpawnLocation = new Vector2I(100, 100);
			playerCavSouth.Units.Insert("Gravebound Charger", 25);
			
			var playerInfantry = new Commander(Provence, "Player");
			// playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			playerInfantry.TargetType = Commander.Target.Closest;
			playerInfantry.SpawnLocation = new Vector2I(100, 50);
			playerInfantry.Units.Insert("Undead Legionary", 150);
			
			var playerInfantry2 = new Commander(Provence, "Player");
			// playerInfantry.CurrentCommand = Commander.Command.HoldAndAttack;
			playerInfantry2.TargetType = Commander.Target.Closest;
			playerInfantry2.SpawnLocation = new Vector2I(100, 50);
			playerInfantry2.Units.Insert("Undead Legionary", 100);
			
			var playerArchers = new Commander(Provence, "Player");
			// playerArchers.CurrentCommand = Commander.Command.HoldAndAttack;
			playerArchers.TargetType = Commander.Target.Random;
			playerArchers.SpawnLocation = new Vector2I(0, 50);
			playerArchers.Units.Insert("Skeleton Marksman", 100);
			
			var enemyInfantry = new Commander(Provence, "Enemy");
			enemyInfantry.SpawnLocation = new Vector2I(100, 50);
			enemyInfantry.Units.Insert("Infantry", 200);
			
			var enemyInfantry2 = new Commander(Provence, "Enemy");
			enemyInfantry2.SpawnLocation = new Vector2I(100, 50);
			enemyInfantry2.Units.Insert("Infantry", 200);
			
			var enemyInfantry3 = new Commander(Provence, "Enemy");
			enemyInfantry3.SpawnLocation = new Vector2I(100, 50);
			enemyInfantry3.Units.Insert("Infantry", 200);
						
			var enemyInfantry4 = new Commander(Provence, "Enemy");
			enemyInfantry4.SpawnLocation = new Vector2I(100, 50);
			enemyInfantry4.Units.Insert("Infantry", 200);
			
			var enemyArchers = new Commander(Provence, "Enemy");
			enemyArchers.TargetType = Commander.Target.Random;
			enemyArchers.SpawnLocation = new Vector2I(0, 50);
			enemyArchers.Units.Insert("Archer", 100);
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
				Mathf.RoundToInt(BattleTileMap.Y * commanderOffset.Y / 100.0f * 8 / 10.0f)
			);
			
			GD.Print(commanderOffsetScaled);
			// -BattleTileMap.Y * 2 / 10 - commanderOffset.Y * 6 / 10
			
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

		if (team == "Player" && Globals.MapScene.TileMap.Provinces.Count(province => province.Owner == "Player") == 2)
		{
			var provenceUp = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 2);
			var provenceDown = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 2);
			var provenceLeft = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 2);
			var provenceRight = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 2);
		}

		if (team == "Player" && Globals.MapScene.TileMap.Provinces.Count(province => province.Owner == "Player") == 3)
		{
			var provenceUp = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Archer", 5);
			var provenceDown = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Archer", 5);
			var provenceLeft = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Archer", 5);
			var provenceRight = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Archer", 5);
			
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 5);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 5);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 5);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 5);
		}
		
		if (team == "Player" && Globals.MapScene.TileMap.Provinces.Count(province => province.Owner == "Player") == 4)
		{
			var provenceUp = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 5);
			var provenceDown = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 5);
			var provenceLeft = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 5);
			var provenceRight = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 5);
		}
		
		Globals.MapScene.UpdateListeners.ForEach(listener => listener());
	}
}
