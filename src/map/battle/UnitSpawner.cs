using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map.character;

public partial class UnitSpawner : Node2D
{
	[Export] private int _spawnCount = 10;
	[Export] private string _team = "Player";
	
	public override void _Ready()
	{
		GetNode<Sprite2D>("Sprite2D").Visible = false;
		CallDeferred("_spawn");
	}

	private void _spawn() {
		if (Globals.BattleScene.Provence != null)
		{
			LoadFromCommanders();
		}
		else
		{
			LoadDefault();
		}
	}

	private void LoadFromCommanders()
	{
		foreach (var commander in Globals.BattleScene.Provence.Commanders
			         .Where(commander => commander.Team == _team))
		{
			foreach (var entry in commander.Units.Items)
			{
				for (int i = 0; i < entry.Value; i++)
				{
					AddUnit(entry.Key, commander);
				}	
			}
		}
	}
	
	private void LoadDefault()
	{
		var position = Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
		Enumerable.Range(0, _spawnCount).ToList()
			.ForEach(_ => AddUnit("Warrior"));
	}

	private void AddUnit(string unitType, Commander commander = null)
	{
		var newUnit = new Unit(unitType, commander);
		var position = Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
		newUnit.Position = Globals.BattleScene.TileMap.MapToGlobal(Globals.BattleScene.TileMap.GetNearestEmpty(position));
		newUnit.Team = _team;
		Globals.BattleScene.TileMap.AddEntity(newUnit.Position, newUnit, BattleTileMap.Unit);
		Globals.BattleScene.CallDeferred("add_child", newUnit);
	}
	
}
