using Godot;
using System;

public partial class CloudRegionSpawner : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < 100; i++)
		{
			var offset = GD.RandRange(0, 6400);
			Spawn(offset);
		}
	}

	public void Spawn(float offset)
	{
		var bottomPoint = new Vector2(-3200, 3200);
		var topPoint = new Vector2(3200, -3200);
		var pointOnLine = bottomPoint.Lerp(topPoint, GD.Randf());
		
		
		var spawn = new CloudTest();
		spawn.GlobalPosition = pointOnLine + spawn.Direction * offset - spawn.Texture.GetSize();
		
		CallDeferred("add_child", spawn);
	}
}
