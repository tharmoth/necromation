using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using Godot.Collections;
using Necromation;
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
		SpawnGrass(startpos);
		
		
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

		var grassTexture = Database.Instance.GetTexture("Grass2");
		var grassTexture2 = Database.Instance.GetTexture("Grass5");
		PropSpawner spawner = new(PropSpawner.RandomType.Particles, new Array<Texture2D>(){ grassTexture, grassTexture2 }, ProvinceSize * TileSize / 2, .75f);
		spawner.GlobalPosition = startpos + Vector2I.One * ProvinceSize * TileSize / 2;
		Globals.FactoryScene.CallDeferred("add_child", spawner);
	}
	
	private void SpawnResource(Vector2I location)
	{
		var startpos = (location) * TileSize * (ProvinceSize + ProvinceGap);
		
		var resources = new List<string> {"Bone Fragments"};
		if ((location - MapScene.FactoryPosition).Length() != 0) 
			resources.AddRange(new List<string> {"Copper Ore", "Coal Ore", "Stone"});
		if ((location - MapScene.FactoryPosition).Length() > 3) resources.Add("Tin Ore");
		
		var resource = resources[GD.RandRange(0, resources.Count - 1)];
		if ((location - MapScene.FactoryPosition).Length() > 3) resource = "Tin Ore";
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

		var spawner = new ResourceSpawner(resource, 3);
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
		
		AddProvence(MapScene.FactoryPosition);
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
