using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using Godot.Collections;
using Necromation;
using Necromation.factory.interactables.interfaces;
using Necromation.map;
using Necromation.sk;
using Resource = Necromation.Resource;

public partial class FactoryTileMap : LayerTileMap
{
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	public static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/shaders/fog_particle.tscn");
	public const string Building = "building";
	public const string Resource = "resource";
	private const int Width = 600;
	private const int Height = 600;
	
	/**************************************************************************
	 * Private Variables                                                      *
	 **************************************************************************/
	private readonly HashSet<Vector2I> _buildable = [];
	private readonly HashSet<Province> _unlockedProvinces = [];
	
	private TileMap _visionTileMap;
	private TileMap VisionTileMap
	{
		get => _visionTileMap ??= GetNode<TileMap>("%VisionTileMap");
		set => _visionTileMap = value;
	}
	private bool _initialized = false;

	/**************************************************************************
	 * Constructor                                                            *
	 **************************************************************************/
	public FactoryTileMap()
	{
		AddLayer(Building);
		AddLayer(Resource);
	}
	
	/**************************************************************************
	 * Public Methods                                                         *
	 **************************************************************************/
	public void OnOpen()
	{
		if (!_initialized)
		{
			_initialized = true;
			Initialize();
		}
		UpdateFogOfWar();
		// Globals.MapScene.TileMap.Provinces
		// 	.Where(province => province.Owner == "Player")
		// 	.Select(province => province.MapPosition)
		// 	.ToList()
		// 	.ForEach(AddProvence);
	}
	
	public override bool IsOnMap(Vector2I mapPos)
	{
		return _buildable.Contains(mapPos);
	}

	public bool IsBuildable(Vector2I mapPos)
	{
		return IsOnMap(mapPos) && Globals.FactoryScene.TileMap.GetEntity(mapPos, Building) == null;
	}
	
	public bool IsResource(Vector2I mapPos)
	{
		return Globals.FactoryScene.TileMap.GetEntity(mapPos, Resource) != null;
	}
	
	public string GetResourceType(Vector2I mapPos)
	{
		var entity = Globals.FactoryScene.TileMap.GetEntity(mapPos, Resource);
		if (entity is Resource resource)
		{
			return resource.Type;
		}

		return null;
	}

	public Building GetBuildingAtMouse()
	{
		return GetEntity(GetGlobalMousePosition(), Building) as Building;
	}
	
	public Resource GetResourceAtMouse()
	{
		return GetEntity(GetGlobalMousePosition(), Resource) as Resource;
	}

	public List<IEntity> GetEntitiesWithinRadius(Vector2I mapPosition, int radius)
	{
		return GetEntities(GetTilesInRadius(mapPosition, radius));
	}

	public List<IEntity> GetEntitiesWithinRect(Rect2I rect)
	{
		List<Vector2I> tiles = new();
		for (int x = rect.Position.X; x < rect.End.X; x++)
		{
			for (int y = rect.Position.Y; y < rect.End.Y; y++)
			{
				tiles.Add(new Vector2I(x, y));
			}
		}

		return GetEntities(tiles);
	}

	private List<IEntity> GetEntities(List<Vector2I> tiles)
	{
		return tiles.Select(cell => GetEntity(cell, Building))
			.Where(entity => entity != null)
			.ToList();
	}
	
	public List<T> GetEntities<T>() where T : class
	{
		return GetEntities(Building).OfType<T>().ToList();
	}
	
	public HashSet<T> GetComponents<T>() where T : class
	{
		return GetEntities(Building)
			.OfType<Building>()
			.Select(building => building.GetComponent<T>())
			.NonNull()
			.ToHashSet();
	}
	
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	private void Initialize()
	{
		for (var y = 0; y < Width; y++)
		{
			for (var x = 0; x < Height; x++)
			{
				var mapPosition = new Vector2I(x, y);
				var prov = Globals.MapScene.Map.GetProvencePercent(new Vector2(x / (float) Width,y / (float) Height));
				if (prov == null) continue;
				
				if (prov.Biome == Biome.Ocean)
				{
					SetCell(0, mapPosition, 2, new Vector2I(4, 0));
				}
				else
				{
					SetCell(0, mapPosition, 2, new Vector2I(0, 0));
					VisionTileMap.SetCell(0, mapPosition, 0, new Vector2I(10, 0));
				}
			}
		}

		TilemapPropSpawner sparseGrassSpawner = new()
		{
			TargetTileMap = this, 
			AtlasCoords = [new Vector2I(0, 0)],  
			Textures = [Database.Instance.GetTexture("Grass2")],
			Threshold = 0.5f,
			Density = 0.5f,
			SizePixels = 24,
		};
		CallDeferred("add_sibling", sparseGrassSpawner);
	}
	
	private void UpdateFogOfWar()
	{
		for (var x = 0; x < Width; x++)
		{
			for (var y = 0; y < Height; y++)
			{
				var mapPosition = new Vector2I(x, y);
				var prov = Globals.MapScene.Map.GetProvencePercent(new Vector2(x / (float) Width,y / (float) Height));
				if (prov == null) continue;
				if (prov.Owner == "Player")
				{
					_buildable.Add(mapPosition);
				}
				if (prov.Biome == Biome.Ocean || prov.Owner == "Player")
				{
					VisionTileMap.SetCell(0, mapPosition);
				}
			}
		}

		Globals.MapScene
			.Map
			.Provinces
			.Where(province => province.Owner == "Player")
			.Except(_unlockedProvinces)
			.ToList()
			.ForEach(newProvince =>
			{
				var resourceSpawner = new ResourceSpawner(newProvince.Resource, 3)
				{
					Position = newProvince.PositionPercent * new Vector2(Width, Height) * TileSet.TileSize,
				};
				CallDeferred("add_child", resourceSpawner);

				_unlockedProvinces.Add(newProvince);
			});
	}
}
