using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Necromation;
using Necromation.map;
using Necromation.sk;

public partial class BuildingTileMap : LayerTileMap
{
	public const string Building = "building";
	public const string Resource = "resource";
	public const int TileSize = 32;
	public const int ProvinceSize = 100;

	private void AddProvence(Vector2I location)
	{
		var startpos = (location - MapGlobals.FactoryPosition) * TileSize * 200;
		
		for (int x = 0; x < ProvinceSize; x++)
		{ 
			for (int y = 0; y < ProvinceSize; y++)
			{
				var coords = new Vector2I(x, y) + GlobalToMap(startpos);
				
				// GD.Print(GetCellAlternativeTile(0));
				
				// get a random vector between 0, 0 and 7, 3

				Vector2I randomvec;
				var random = GD.Randf();
				if (random < 0.5)
				{
					randomvec = Vector2I.Zero;
				} else if (random < 0.9)
				{
					randomvec = new Vector2I(GD.RandRange(0, 3), GD.RandRange(0, 3));
				} else
				{
					randomvec = new Vector2I(GD.RandRange(4, 7), GD.RandRange(0, 3));
				}
				

				SetCell(0, coords, 0, randomvec);
			}
		}
	}

	public override void _EnterTree()
	{
		Globals.TileMap = this;
		// TODO: refactor this into a collision mask instead of discrete layers?
		AddLayer(Building);
		AddLayer(Resource);

		// TODO: Only load this when needed
		for (int x = 0; x < 9; x++)
		{
			for (int y = -2; y < 3; y++)
			{
				AddProvence(new Vector2I(x, y));
			}
		}
	}

	public bool IsOnMap(Vector2I mapPos)
	{
		return GetCellSourceId(0, mapPos) != -1;
	}
	
	public bool IsBuildable(Vector2I mapPos)
	{
		return IsOnMap(mapPos) && Globals.TileMap.GetEntity(mapPos, Building) == null;
	}
	
	public bool IsResource(Vector2I mapPos)
	{
		return Globals.TileMap.GetEntity(mapPos, Resource) != null;
	}

	public IEntity GetBuildingAtMouse()
	{
		return GetEntity(GetGlobalMousePosition(), Building);
	}
	
	public IEntity GetResourceAtMouse()
	{
		return GetEntity(GetGlobalMousePosition(), Resource);
	}
}
