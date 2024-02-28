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
	[Export] private string _defaultUnit = "Infantry";
	private Queue<Unit> _units = new();
	
	private readonly Commander _commander;
	private readonly string _team = "Player";

	public UnitSpawner()
	{

	}
	
	public UnitSpawner(Commander commander, string team)
	{
		_commander = commander;
		_team = team;
	}
	
	public override void _Ready()
	{
		if (GetNode<Sprite2D>("Sprite2D") is { } sprite2D)
		{
			sprite2D.Visible = false;
		}
		
		CallDeferred("_spawn");
	}

	private void _spawn() {
		LoadFromCommanders();
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
		foreach (var entry in _commander.Units.Items)
		{
			for (int i = 0; i < entry.Value; i++)
			{
				_units.Enqueue(new Unit(entry.Key, _commander));
			}	
		}
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
