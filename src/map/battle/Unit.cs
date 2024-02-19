using Godot;
using System;

public partial class Unit : Sprite2D
{
	private double _time = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GlobalPosition = BattleGlobals.TileMap.ToMap(GlobalPosition);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Every 0.5 seconds, move the unit one cell forward
		_time += delta;
		if (_time < 0.5) return;
		_time = 0;

		var position =BattleGlobals.TileMap.GlobalToMap(GlobalPosition);
		GlobalPosition = BattleGlobals.TileMap.MapToGlobal(position + SKTileMap.GetRight());
	}
}
