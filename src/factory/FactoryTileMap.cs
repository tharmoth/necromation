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
	public const string Building = "building";
	public const string Resource = "resource";
	public const int TileSize = 32;
	public const int ProvinceSize = 20;
	public const int ProvinceGap = 0;
	private readonly List<Vector2I> _provences = new();
	private readonly System.Collections.Generic.Dictionary<Vector2I, Node2D> _fogs = new();
	
	public static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/shaders/fog_particle.tscn");
	
	private readonly HashSet<Vector2I> _buildable = new();

	private void AddFog(Vector2I location)
	{
		// We're missing a fog of war texture for now.
		// if (true) return;
		
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);
		
		Sprite2D sprite = new();
		sprite.Texture = Database.Instance.GetTexture("fow2");
		var scaler = (TileSize * ProvinceSize) / (sprite.Texture.GetSize().X - 192);
		sprite.Scale = new Vector2(scaler, scaler);
		sprite.GlobalPosition = startpos + scaler * sprite.Texture.GetSize() / 2 - Vector2.One * 96;
		sprite.Centered = true;
		sprite.ZIndex = 1000;
		CanvasItemMaterial material = new();
		material.BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha;
		sprite.Material = material;
		Globals.FactoryScene.CallDeferred("add_child", sprite);
		
		// sprite.RotationDegrees = new List<float> { 0, 90, 180, 270 }[GD.RandRange(0, 3)];
		// Globals.FactoryScene.CallDeferred("add_child", sprite);
		_fogs.Add(location, sprite);
		
		SpawnGrass(location);
	}
	
	public void AddProvence(Vector2I location)
	{
		AddProvence(location, true);
	}
	
	public void AddProvence(Vector2I location, bool spawnResource)
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

				_buildable.Add(coords);
				
				// SetCell(0, coords, 0, randomvec);
			}
		}
		// SpawnGrass(startpos);
		
		if (_fogs.TryGetValue(location, out var fog)) fog?.QueueFree();
	}

	private void SpawnBackgroundSprite(Vector2I location, string name)
	{
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);
		
		Sprite2D soilSprite = new();
		soilSprite.Texture = Database.Instance.GetTexture(name);
		var scaler = (TileSize * ProvinceSize) / soilSprite.Texture.GetSize().X;
		soilSprite.Scale = new Vector2(scaler, scaler);
		soilSprite.GlobalPosition = startpos + scaler * soilSprite.Texture.GetSize() / 2;
		soilSprite.Centered = true;
		soilSprite.ZIndex = -99;

		if (name == "Ocean")
		{
			soilSprite.Material = GD.Load<ShaderMaterial>("res://src/factory/shaders/wiggle.tres");
		}
		
		Globals.FactoryScene.CallDeferred("add_child", soilSprite);
	}

	private void SpawnGrass(Vector2I location)
	{
		SpawnBackgroundSprite(location, "grasstile");
		
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);
		var propPos = startpos + Vector2I.One * ProvinceSize * TileSize / 2;

		PropSpawner spawner = new();
		spawner.Textures.Add(Database.Instance.GetTexture("Grass2"));
		spawner.Textures.Add(Database.Instance.GetTexture("Grass5"));
		spawner.Density = 0.5f;
		spawner.Radius = ProvinceSize * TileSize / 2;
		spawner.SizePixels = 24;
		spawner.Threshold = .5f;
		spawner.GlobalPosition = propPos;
		spawner.ZIndex = -97;
		Globals.FactoryScene.CallDeferred("add_child", spawner);

		PropSpawner rockSpawner = new();
		rockSpawner.Textures.Add(Database.Instance.GetTexture("Rocks1"));
		rockSpawner.Textures.Add(Database.Instance.GetTexture("Rocks3"));
		rockSpawner.Density = 0.25f;
		rockSpawner.Radius = ProvinceSize * TileSize / 2;
		rockSpawner.SizePixels = 32;
		rockSpawner.Threshold = .75f;
		rockSpawner.GlobalPosition = propPos;
		rockSpawner.ZIndex = -98;
		rockSpawner.UseWind = false;
		rockSpawner.Single = true;
		Globals.FactoryScene.CallDeferred("add_child", rockSpawner);
		
		TreeSpawner treeSpawner = new();
		treeSpawner.NoiseSeed = 1;
		treeSpawner.Threshold = .75f;
		treeSpawner.Radius = ProvinceSize * TileSize / 2;
		treeSpawner.GlobalPosition = propPos;
		// Globals.FactoryScene.CallDeferred("add_child", treeSpawner);

		BushSpawner bushSpawner = new();
		bushSpawner.GlobalPosition = propPos;
		bushSpawner.Radius = ProvinceSize * TileSize / 2;
		// Globals.FactoryScene.CallDeferred("add_child", bushSpawner);
	}
	
	private void SpawnResource(Vector2I location)
	{
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);

		var dist = location.DistanceTo(MapScene.FactoryPosition);
		var amount = dist > 2 ? 7 : 3;

		var province = Globals.MapScene.TileMap.Provinces
			.First(province => province.MapPosition == location);
		
		var spawnerX = GD.RandRange(4, ProvinceSize - 4) * TileSize;
		var spawnerY = GD.RandRange(4, ProvinceSize - 4) * TileSize;
		var spawner = new ResourceSpawner(province.Resource, amount);
		spawner.GlobalPosition = ToGlobal(startpos + new Vector2(spawnerX, spawnerY));
		Globals.FactoryScene.CallDeferred("add_child", spawner);
	}

	public void OnOpen()
	{
		Globals.MapScene.TileMap.Provinces
			.Where(province => province.Owner == "Player")
			.Select(province => province.MapPosition)
			.ToList()
			.ForEach(AddProvence);
	}

	public override void _EnterTree()
	{
		// TODO: refactor this into a collision mask instead of discrete layers?
		AddLayer(Building);
		AddLayer(Resource);
		
		foreach (var province in Globals.MapScene.TileMap.Provinces)
		{
			AddFog(province.MapPosition);
		}
		
		foreach (var province in Globals.MapScene.TileMap.GetOcean())
		{
			SpawnBackgroundSprite(province, "Ocean");
		}
		
		foreach (var province in Globals.MapScene.TileMap.GetMountain())
		{
			SpawnBackgroundSprite(province, "Snow");
		}
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
}
