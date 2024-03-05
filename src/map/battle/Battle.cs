using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class Battle : Scene
{
	public const double TimeStep = .5;
	public BattleTileMap TileMap => GetNode<BattleTileMap>("%TileMap");
	public Province Provence;

	public override void _Ready()
	{
		base._Ready();
		MusicManager.PlayBattleMusic();
			
		if (Provence == null) Provence = new Province();
		
		if (Provence.Commanders.Count == 0)
		{
			var PlayerCommanderInfantry = new Commander(new Province(), "Player");
			PlayerCommanderInfantry.Units.Insert("Skeleton Barbarian", 1000);
			Provence.Commanders.Add(PlayerCommanderInfantry);
			
			var EnemyCommanderInfantry = new Commander(new Province(), "Enemy");
			EnemyCommanderInfantry.Units.Insert("Heavy Infantry", 100);
			Provence.Commanders.Add(EnemyCommanderInfantry);
			
			// var PlayerCommanderArcher = new Commander(new Province(), "Player");
			// PlayerCommanderArcher.SpawnLocation = new Vector2I(0, 25);
			// PlayerCommanderArcher.Units.Insert("Archer", 100);
			// Provence.Commanders.Add(PlayerCommanderArcher);
			
			// var EnemyCommanderArcher = new Commander(new Province(), "Enemy");
			// EnemyCommanderArcher.SpawnLocation = new Vector2I(30, 25);
			// EnemyCommanderArcher.Units.Insert("Archer", 100);
			// Provence.Commanders.Add(EnemyCommanderArcher);
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

	public override void OnOpen()
	{
		Provence = Globals.MapScene.SelectedProvince;
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
		var deadCommanders =
			Globals.BattleScene.Provence.Commanders.Where(commander => commander.Team != team).ToList();
		deadCommanders.ForEach(commander => Globals.BattleScene.Provence.Commanders.Remove(commander));
		deadCommanders.ForEach(commander => commander.QueueFree());
		Globals.BattleScene.QueueFree();

		if (team == "Player" && Globals.MapScene.TileMap.GetProvinces().Count(province => province.Owner == "Player") == 2)
		{
			var provenceUp = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 2);
			var provenceDown = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 2);
			var provenceLeft = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 2);
			var provenceRight = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 2);
		}

		if (team == "Player" && Globals.MapScene.TileMap.GetProvinces().Count(province => province.Owner == "Player") == 3)
		{
			var provenceUp = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Archer", 5);
			var provenceDown = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Archer", 5);
			var provenceLeft = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Archer", 5);
			var provenceRight = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Archer", 5);
			
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 5);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 5);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 5);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 5);
		}
		
		if (team == "Player" && Globals.MapScene.TileMap.GetProvinces().Count(province => province.Owner == "Player") == 4)
		{
			var provenceUp = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 5);
			var provenceDown = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 5);
			var provenceLeft = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 5);
			var provenceRight = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 5);
		}
		
		Globals.MapScene.UpdateListeners.ForEach(listener => listener());
	}
}
