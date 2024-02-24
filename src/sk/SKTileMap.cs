using System;
using System.Collections.Generic;
using Godot;

public partial class SKTileMap : TileMap
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}

	public static Vector2I GetDirection(Direction direction)
	{
		return direction switch
		{
			Direction.Up => new Vector2I(0, -1),
			Direction.Down => new Vector2I(0, 1),
			Direction.Left => new Vector2I(-1, 0),
			Direction.Right => new Vector2I(1, 0),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public static Vector2I GetRight()
	{
		return GetDirection(Direction.Right);
	}
	
	public static Vector2I GetLeft()
	{
		return GetDirection(Direction.Left);
	}
	
	public static Vector2I GetUp()
	{
		return GetDirection(Direction.Up);
	}
	
	public static Vector2I GetDown()
	{
		return GetDirection(Direction.Down);
	}
	
	public Vector2I GlobalToMap(Vector2 point)
	{
		return LocalToMap(ToLocal(point));
	}
	
	public Vector2 MapToGlobal(Vector2I point)
	{
		return ToGlobal(MapToLocal(point));
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
