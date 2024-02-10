using Godot;
using System;
using Necromation;

public partial class Spawner : Node2D
{
	[Export] private PackedScene _type;
	[Export] private string _resourceType = "Stone";
	[Export] private int _radius = 3;
	
	private double _time;

	public override void _Ready()
	{
		base._Ready();
		GetNode<Sprite2D>("Sprite2D").Visible = false;

		CallDeferred("_spawn");
	}

	private void _spawn()
	{
		var mapPos = Globals.TileMap.GlobalToMap(GlobalPosition);
		for (var x = -_radius; x <= _radius; x++)
		{
			for (var y = -_radius; y <= _radius; y++)
			{
				// If the distance from the origin is more than five, skip
				if (Math.Sqrt(x * x + y * y) > _radius) continue;
				// Randomly skip some tiles with a frequency increasing closer to the edge
				if (new Random().NextDouble() > 1.5 - Math.Sqrt(x * x + y * y) / _radius) continue;
				
				var spawn = _type.Instantiate<Collectable>();
				spawn.Type = _resourceType;
				Globals.TileMap.AddEntity(mapPos + new Vector2I(x, y), spawn, BuildingTileMap.LayerNames.Resources);
			}
		}
	}
}
