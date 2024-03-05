using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using Godot.Collections;
using Necromation;
using Necromation.character;
using Necromation.map;
using Necromation.sk;

public partial class BuildingTileMap : LayerTileMap
{
	public const string Building = "building";
	public const string Resource = "resource";
	public const int TileSize = 32;
	public const int ProvinceSize = 20;
	public const int ProvinceGap = 0;
	private readonly List<Vector2I> _provences = new();

	public void AddProvence(Vector2I location)
	{
		AddProvence(location, true);
	}
	
	private void AddProvence(Vector2I location, bool spawnResource)
	{
		if (_provences.Contains(location)) return;
		_provences.Add(location);
		
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);

		if (spawnResource) SpawnResource(location);
		
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
		
		Sprite2D sprite = new();
		sprite.Texture = Globals.Database.GetTexture("soil2");
		var scaler = (TileSize * ProvinceSize) / sprite.Texture.GetSize().X;
		sprite.Scale = new Vector2(scaler, scaler);
		sprite.GlobalPosition = startpos;
		sprite.Centered = false;
		sprite.ZIndex = -99;
		Globals.FactoryScene.CallDeferred("add_child", sprite);

		var grassTexture = Globals.Database.GetTexture("Grass2");
		var grassTexture2 = Globals.Database.GetTexture("Grass5");
		PropSpawner spawner = new(PropSpawner.RandomType.Cuboid, new Array<Texture2D>(){ grassTexture, grassTexture2 }, ProvinceSize * TileSize / 2, .5f);
		spawner.GlobalPosition = startpos + Vector2I.One * ProvinceSize * TileSize / 2;
		Globals.FactoryScene.CallDeferred("add_child", spawner);
	}
	
	private void SpawnResource(Vector2I location)
	{
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);
		
		var resources = new List<string> {"Bone Fragments"};
		if ((location - MapGlobals.FactoryPosition).Length() != 0) 
			resources.AddRange(new List<string> {"Copper Ore", "Coal Ore", "Stone"});
		if ((location - MapGlobals.FactoryPosition).Length() > 3) resources.Add("Tin Ore");
		
		var resource = resources[GD.RandRange(0, resources.Count - 1)];
		if ((location - MapGlobals.FactoryPosition).Length() > 3) resource = "Tin Ore";
		switch (_provences.Count)
		{
			case 1:
				resource = "Bone Fragments";
				break;
			case 2:
				resource = "Stone";
				break;
			case 3:
				resource = "Coal Ore";
				break;
			case 4:
				MusicManager.PlayExploration();
				resource = "Copper Ore";
				break;
		}
		
		var spawnerX = GD.RandRange(4, ProvinceSize - 4) * TileSize;
		var spawnerY = GD.RandRange(4, ProvinceSize - 4) * TileSize;

		var spawner = new Spawner(resource, 3);
		spawner.GlobalPosition = ToGlobal(startpos + new Vector2(spawnerX, spawnerY));
		Globals.FactoryScene.CallDeferred("add_child", spawner);
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
	
	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		var dict =  new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", "BuildingTilemap" },
		};
		foreach (var provence in _provences)	
		{
			dict["Provence" + _provences.IndexOf(provence) + "X"] = provence.X;
			dict["Provence" + _provences.IndexOf(provence) + "Y"] = provence.Y;
		}
		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		for (var i = 0; i < nodeData.Count; i++)
		{
			if (!nodeData.ContainsKey("Provence" + i + "X")) continue;
			var province = new Vector2I((int)nodeData["Provence" + i + "X"], (int)nodeData["Provence" + i + "Y"]);
			MapGlobals.TileMap.GetProvence(province).Commanders.Clear();
			MapGlobals.TileMap.GetProvence(province).Owner = "Player";
			Globals.TileMap.AddProvence(province, false);
		}
	}
}
