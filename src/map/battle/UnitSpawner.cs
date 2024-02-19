using Godot;
using System;

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
		var position = BattleGlobals.TileMap.GlobalToMap(GlobalPosition);
		for (var i = 0; i < _spawnCount; i++)
		{
			var newUnit = new Unit();
			newUnit.Position = BattleGlobals.TileMap.MapToGlobal(position + new Vector2I(0, i));
			newUnit.Team = _team;
			GD.Print("Spawning unit at" + (position + new Vector2I(0, i)));
			GetTree().Root.CallDeferred("add_child", newUnit);
		}
	}
}
