using System;
using System.Collections.Generic;
using Godot;

public partial class SKTileMap : TileMap
{
	public Vector2I GlobalToMap(Vector2 point)
	{
		return LocalToMap(ToLocal(point));
	}
	
	public Vector2 MapToGlobal(Vector2I point)
	{
		var local = MapToLocal(point);
		var global = ToGlobal(local);
		return global;
	}

	public Vector2 ToMap(Vector2 point)
	{
		return MapToGlobal(GlobalToMap(point));
	}
	
	// See https://www.redblobgames.com/grids/circle-drawing/
	public List<Vector2I> GetTilesInRadius(Vector2I center, float radius)
	{
		List<Vector2I> result = new();
		int top    =  Mathf.CeilToInt(center.Y - radius),
			bottom = Mathf.FloorToInt(center.Y + radius);

		for (int y = top; y <= bottom; y++) {
			int   dy  = y - center.Y;
			float dx  = Mathf.Sqrt(radius*radius - dy*dy);
			int left  = Mathf.CeilToInt(center.X - dx),
				right = Mathf.FloorToInt(center.X + dx);
			for (int x = left; x <= right; x++) {
				result.Add(new Vector2I(x, y));
			}
		}

		return result;
	}
}
