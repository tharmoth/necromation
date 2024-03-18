using Godot;
using System;

public partial class CloudRegionSpawner : Control
{
	
			
	float xSize = BattleTileMap.TileSize * BattleTileMap.X;
	float ySize = BattleTileMap.TileSize * BattleTileMap.Y;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		
		for (int i = 0; i < 750; i++)
		{
			var offset = (float) GD.RandRange(0.0f, ySize + xSize);
			Spawn(offset);
		}
	}

	public void Spawn(float offset)
	{
		var bottomPoint = new Vector2(-xSize, xSize);
		var topPoint = new Vector2(ySize, -ySize);
		var pointOnLine = bottomPoint.Lerp(topPoint, GD.Randf());
		
		
		var spawn = new CloudTest();
		spawn.GlobalPosition = pointOnLine + spawn.Direction * offset - spawn.Texture.GetSize() * spawn.Scale.X;
		
		CallDeferred("add_child", spawn);
	}
}
