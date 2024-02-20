using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Necromation;
using Necromation.sk;

public partial class BuildingTileMap : LayerTileMap
{
	public const string Building = "building";
	public const string Resource = "resource";
	public const int TileSize = 32;

	public override void _EnterTree(){
		Globals.TileMap = this;
		// TODO: refactor this into a collision mask instead of discrete layers?
		AddLayer(Building);
		AddLayer(Resource);
	}

	public bool IsBuildable(Vector2I mapPos)
	{
		return GetCellSourceId(0, mapPos) != -1 && Globals.TileMap.GetEntities(mapPos, Building) == null;
	}
	
	public bool IsResource(Vector2I mapPos)
	{
		return Globals.TileMap.GetEntities(mapPos, Resource) != null;
	}

	public IEntity GetBuildingAtMouse()
	{
		return GetEntities(GetGlobalMousePosition(), Building);
	}
	
	public IEntity GetResourceAtMouse()
	{
		return GetEntities(GetGlobalMousePosition(), Resource);
	}
}
