using Godot;

public partial class SKTileMap : TileMap
{
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
}
