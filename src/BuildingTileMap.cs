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
	public const int ProvinceSize = 20;
	public const int ProvinceGap = 0;
	private readonly Dictionary<Vector2I, Province> _provences = new();

	public void AddProvence(Vector2I location)
	{
		if (_provences.ContainsKey(location)) return;
		_provences.Add(location, new Province());
		
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);
		
		var resources = new List<string> {"Bone Fragments"};
		if ((location - MapGlobals.FactoryPosition).Length() != 0) 
			resources.AddRange(new List<string> {"Copper Ore", "Coal Ore", "Stone"});
		if ((location - MapGlobals.FactoryPosition).Length() > 3) resources.Add("Tin Ore");
		var resource = resources[GD.RandRange(0, resources.Count - 1)];

		if (_provences.Count == 1) resource = "Bone Fragments";
		if (_provences.Count == 2) resource = "Stone";
		if (_provences.Count == 3) resource = "Coal Ore";
		if (_provences.Count == 4) resource = "Copper Ore";
		
		var spawner = new Spawner(resource, 3);
		spawner.GlobalPosition = ToGlobal(startpos + new Vector2I(ProvinceSize / 2, ProvinceSize / 2) * TileSize);
		Globals.FactoryScene.CallDeferred("add_child", spawner);

		for (int x = 0; x < ProvinceSize; x++)
		{ 
			for (int y = 0; y < ProvinceSize; y++)
			{
				var coords = new Vector2I(x, y) + GlobalToMap(startpos);
				
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
		
		AddProvence(MapGlobals.FactoryPosition);
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
