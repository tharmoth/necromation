using Godot;
using System;

public partial class CloudRegionSpawner : Control
{
	
			
	float xSize = BattleTileMap.TileSize * BattleTileMap.X;
	float ySize = BattleTileMap.TileSize * BattleTileMap.Y;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var count = Mathf.RoundToInt(0.012 * BattleTileMap.X * BattleTileMap.Y);
		
		for (int i = 0; i < count; i++)
		{
			var offset = (float) GD.RandRange(0.0f, ySize + xSize);
			// Spawn(offset);
		}
		
		CustomMinimumSize = new Vector2(xSize, ySize);

		var sprite = GD.Load<PackedScene>("res://src/factory/shaders/cloudsprite.tscn").Instantiate<Sprite2D>();
		var maxScale = Mathf.Max(xSize, ySize);
		var minSize = Mathf.Min(sprite.Texture.GetHeight(), sprite.Texture.GetHeight());
		var scale = maxScale / minSize;
		sprite.Scale = new Vector2(scale, scale);
		
		
        AddChild(sprite);
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
