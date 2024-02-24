using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map.battle;
using Necromation.map.character;

public partial class UnitSpawner : Node2D
{
	[Export] private int _spawnCount = 10;
	[Export] private string _team = "Player";
	[Export] private string _defaultUnit = "Warrior";
	private Queue<Unit> _units = new();
	
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

		PlaceUnitsInRectangle();
	}

	private void PlaceUnitsInRectangle()
	{
		var count = _units.Count;
		var width = (int)Math.Ceiling(Math.Sqrt(count / 2.0f));
		var height = width * 2;
		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				_units.TryDequeue(out var unit);
				if (unit == null) return;;
				
				var position = Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
				position.X += x;
				position.Y += y;
				unit.Position = Globals.BattleScene.TileMap.MapToGlobal(Globals.BattleScene.TileMap.GetNearestEmpty(position));
				unit.Team = _team;
				Globals.BattleScene.TileMap.AddEntity(unit.Position, unit, BattleTileMap.Unit);
				Globals.BattleScene.CallDeferred("add_child", unit);
			}
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
					_units.Enqueue(new Unit(entry.Key, commander));
				}	
			}
		}
	}
	
	private void LoadDefault()
	{
		var position = Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
		Enumerable.Range(0, _spawnCount).ToList()
			.ForEach(_ => _units.Enqueue(new Unit(_defaultUnit)));
	}

	private void AddUnit(string unitType, Commander commander = null)
	{
		Unit newUnit = new Unit(unitType, commander);
		
		var position = Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
		newUnit.Position = Globals.BattleScene.TileMap.MapToGlobal(Globals.BattleScene.TileMap.GetNearestEmpty(position));
		newUnit.Team = _team;
		Globals.BattleScene.TileMap.AddEntity(newUnit.Position, newUnit, BattleTileMap.Unit);
		Globals.BattleScene.CallDeferred("add_child", newUnit);
	}
	
}
