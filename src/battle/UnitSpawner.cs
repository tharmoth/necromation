using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map.battle;
using Necromation.map.character;

public partial class UnitSpawner : Node2D
{
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
		if (HasNode("Sprite2D"))
		{
			GetNode<Sprite2D>("Sprite2D").Visible = false;
		}
		
		CallDeferred("_spawn");
	}

	private void _spawn() {
		PlaceUnitsInRectangle();
	}

	private void PlaceUnitsInRectangle()
	{
		var positions = GetPositions(_commander.Units.CountItems());
		
		foreach (var entry in _commander.Units.Items)
		{
			for (int i = 0; i < entry.Value; i++)
			{
				var position = positions.Dequeue();
				var empty = Globals.BattleScene.TileMap.GetNearestEmpty(position);
				var global = Globals.BattleScene.TileMap.MapToGlobal(empty);
				
				var unit = new Unit(entry.Key, global,  _commander);

				Globals.UnitManager.AddUnit(unit);
			}	
		}
	}

	private Queue<Vector2I> GetPositions(int count)
	{
		Queue<Vector2I> positions = new();
		var width = (int)Math.Ceiling(Math.Sqrt(count / 2.0f));
		var height = width * 2;
		width *= 2;
		height *= 2;
		for (var x = 0; x < width; x += 2)
		{
			for (var y = 0; y < height; y += 2)
			{
				var position = Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
				position.X -= width / 2;
				position.Y -= height / 2;
				position.X += x;
				position.Y += y;
				positions.Enqueue(position);
			}

		}
		return positions;
	}
}
