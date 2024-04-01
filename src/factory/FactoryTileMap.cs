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
		
		SpawnGrass(startpos);
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

	private void SpawnGrass(Vector2I startpos)
	{
		Sprite2D soilSprite = new();
		soilSprite.Texture = Database.Instance.GetTexture("soil2");
		var scaler = (TileSize * ProvinceSize) / soilSprite.Texture.GetSize().X;
		soilSprite.Scale = new Vector2(scaler, scaler);
		soilSprite.GlobalPosition = startpos + scaler * soilSprite.Texture.GetSize() / 2;
		soilSprite.Centered = true;
		soilSprite.ZIndex = -99;
		// We need to fix the edges to enable rotation.
		// sprite.RotationDegrees = new List<float> { 0, 90, 180, 270 }[GD.RandRange(0, 3)];
		Globals.FactoryScene.CallDeferred("add_child", soilSprite);
		
		var propPos = startpos + Vector2I.One * ProvinceSize * TileSize / 2;

		PropSpawner spawner = new();
		spawner.Textures.Add(Database.Instance.GetTexture("Grass2"));
		spawner.Textures.Add(Database.Instance.GetTexture("Grass5"));
		spawner.Density = 0.5f;
		spawner.Radius = ProvinceSize * TileSize / 2;
		spawner.SizePixels = 24;
		spawner.Threshold = .5f;
		spawner.GlobalPosition = propPos;
		spawner.ZIndex = -98;
		Globals.FactoryScene.CallDeferred("add_child", spawner);

		PropSpawner rockSpawner = new();
		rockSpawner.Textures.Add(Database.Instance.GetTexture("Rocks1"));
		rockSpawner.Textures.Add(Database.Instance.GetTexture("Rocks3"));
		rockSpawner.Density = 0.25f;
		rockSpawner.Radius = ProvinceSize * TileSize / 2;
		rockSpawner.SizePixels = 32;
		rockSpawner.Threshold = .75f;
		rockSpawner.GlobalPosition = propPos;
		rockSpawner.ZIndex = -97;
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

		var province = Globals.MapScene.TileMap.Provinces
			.First(province => province.MapPosition == location);
		
		var spawnerX = GD.RandRange(4, ProvinceSize - 4) * TileSize;
		var spawnerY = GD.RandRange(4, ProvinceSize - 4) * TileSize;
		var spawner = new ResourceSpawner(province.Resource, 3);
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

	public Building GetBuildingAtMouse()
	{
		return GetEntity(GetGlobalMousePosition(), Building) as Building;
	}
	
	public Resource GetResourceAtMouse()
	{
		return GetEntity(GetGlobalMousePosition(), Resource) as Resource;
	}
}
