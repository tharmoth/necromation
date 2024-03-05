using Godot;
using System;
using Necromation;
using Resource = Necromation.Resource;

public partial class ResourceSpawner : Node2D
{
	private string _resourceType;
	private int _radius;

	public ResourceSpawner(string resourceType, int radius)
	{
		_resourceType = resourceType;
		_radius = radius;
	}

	public override void _Ready()
	{
		base._Ready();

		CallDeferred("_spawn");
		
		QueueFree();
	}

	private void _spawn()
	{
		var mapPos = Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
		for (var x = -_radius; x <= _radius; x++)
		{
			for (var y = -_radius; y <= _radius; y++)
			{
				// If the distance from the origin is more than five, skip
				if (Math.Sqrt(x * x + y * y) > _radius) continue;
				// Randomly skip some tiles with a frequency increasing closer to the edge
				if (new Random().NextDouble() > 1.5 - Math.Sqrt(x * x + y * y) / _radius) continue;
				
				var spawn = new Resource(_resourceType);
				spawn.GlobalPosition = Globals.FactoryScene.TileMap.MapToGlobal(mapPos + new Vector2I(x, y));
				Globals.FactoryScene.AddChild(spawn);
			}
		}
	}
}
