using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

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
		if (BattleGlobals.Provence != null)
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
		var position = BattleGlobals.TileMap.GlobalToMap(GlobalPosition);
		var index = 0;
		BattleGlobals.Provence.Commanders
			.Where(commander => commander.Team == _team)
			.SelectMany(commander => commander.Units.Items.ToList()).ToList().ForEach(entry =>
		{
			for (var i = 0; i < entry.Value; i++)
			{
				AddUnit(position + new Vector2I(0, index));
				index++;
			}
		});
	}
	
	private void LoadDefault()
	{
		var position = BattleGlobals.TileMap.GlobalToMap(GlobalPosition);
		for (var i = 0; i < _spawnCount; i++)
		{
			AddUnit(position + new Vector2I(0, i));
		}
	}

	private void AddUnit(Vector2I position)
	{
		var newUnit = new Unit();
		newUnit.Position = BattleGlobals.TileMap.MapToGlobal(position);
		newUnit.Team = _team;
		Globals.BattleScene.CallDeferred("add_child", newUnit);
	}
	
}
