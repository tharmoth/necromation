using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class MapTileMap : SKTileMap
{
	/**************************************************************************
	 * Utility Property                                                       *
	 **************************************************************************/
	public List<Province> Provinces => _provences.Values.ToList();
	
	/**************************************************************************
	 * State Variables                                                        *
	 **************************************************************************/
	private readonly System.Collections.Generic.Dictionary<Vector2I, Province> _provences = new();
	private readonly System.Collections.Generic.Dictionary<Vector2I, Node2D> _fogs = new();
	
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	public const int TileSize = 32;
	
	public override void _Ready()
	{
		base._Ready();
		
		foreach (var location in GetUsedCells(0))
		{
			var provence = new Province();
			_provences.Add(location, provence);
			
			var team = location == MapScene.FactoryPosition ? "Player" : "Enemy";
			provence.Owner = team;

			InitCommanders(provence);
			Globals.MapScene.CallDeferred("add_child", provence);
		}
		
		foreach (var province in Globals.MapScene.TileMap.Provinces)
		{
			AddFog(province.MapPosition);
		}
		
		if (_fogs.TryGetValue(MapScene.FactoryPosition, out var fog)) fog?.QueueFree();
	}

	/**************************************************************************
	 * Initialization                                                         *
	 **************************************************************************/
	private static void InitCommanders(Province province)
	{
        if (province.Owner == "Player" && Globals.FactoryScene != null) return;
		
        var meleeCommander = new Commander(province, province.Owner);
        var meleeCommander2 = new Commander(province, province.Owner);
        var meleeCommander3 = new Commander(province, province.Owner);
        var rangedCommander = new Commander(province, province.Owner);
        rangedCommander.SpawnLocation = new Vector2I(30, 25);
		
        var provinceLocation = province.MapPosition;
        var dis = Mathf.Abs(provinceLocation.X - MapScene.FactoryPosition.X) + Mathf.Abs(provinceLocation.Y - MapScene.FactoryPosition.Y);
		
        switch (dis)
        {
            case 0:
                break;
            case 1:
                meleeCommander.Units.Insert("Rabble", 10);
                break;
            case 2:
                meleeCommander.Units.Insert("Infantry", 10);
                rangedCommander.Units.Insert("Archer", 10);
                break;
            case 3:
                meleeCommander.Units.Insert("Infantry", 50);
                rangedCommander.Units.Insert("Archer", 25);
                break;
            case 4:
                meleeCommander.Units.Insert("Infantry", 100);
                rangedCommander.Units.Insert("Archer", 50);
                break;
            case 5:
                meleeCommander.Units.Insert("Barbarian", 200);
                meleeCommander2.Units.Insert("Heavy Infantry", 50);
                rangedCommander.Units.Insert("Archer", 100);
                break;
            case 6:
                meleeCommander.Units.Insert("Elite Infantry", 200);
                meleeCommander2.Units.Insert("Barbarian", 200);
                meleeCommander3.Units.Insert("Barbarian", 200);
                break;
        }
	}
	
	public Province GetProvence(Vector2I position)
	{
		return _provences.TryGetValue(position, out var provence) ? provence : null;
	}

	public Vector2I GetLocation(Province provence)
	{
		return _provences.FirstOrDefault(pair => pair.Value == provence).Key;
	}
	
	
	private void AddFog(Vector2I location)
	{
		// We're missing a fog of war texture for now.
		// if (true) return;
		
		var startpos = (location) * TileSize * (1 + 0);
		
		Sprite2D sprite = new();
		sprite.Texture = Database.Instance.GetTexture("fow2");
		var scaler = (TileSize * 1) / (sprite.Texture.GetSize().X - 192);
		sprite.Scale = new Vector2(scaler, scaler);
		sprite.GlobalPosition = startpos + scaler * sprite.Texture.GetSize() / 2 - scaler * Vector2.One * 96;
		sprite.RotationDegrees = new List<float> { 0, 90, 180, 270 }[GD.RandRange(0, 3)];
		sprite.Centered = true;
		sprite.ZIndex = 1000;
		CanvasItemMaterial material = new();
		material.BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha;
		sprite.Material = material;
		Globals.MapScene.CallDeferred("add_child", sprite);
		_fogs.Add(location, sprite);
		SpawnGrass(startpos);
	}
	
	private void SpawnGrass(Vector2I startpos)
	{
		Sprite2D soilSprite = new();
		soilSprite.Texture = Database.Instance.GetTexture("soil2");
		var scaler = (TileSize * 1) / soilSprite.Texture.GetSize().X;
		soilSprite.Scale = new Vector2(scaler, scaler);
		soilSprite.GlobalPosition = startpos + scaler * soilSprite.Texture.GetSize() / 2;
		soilSprite.Centered = true;
		soilSprite.ZIndex = -99;
		// We need to fix the edges to enable rotation.
		// sprite.RotationDegrees = new List<float> { 0, 90, 180, 270 }[GD.RandRange(0, 3)];
		Globals.MapScene.CallDeferred("add_child", soilSprite);

		var grassTexture = Database.Instance.GetTexture("Grass2");
		var grassTexture2 = Database.Instance.GetTexture("Grass5");
		PropSpawner spawner = new(PropSpawner.RandomType.Particles, new Array<Texture2D>(){  }, 1 * TileSize / 2, .75f);
		 spawner.GlobalPosition = startpos + Vector2I.One * 1 * TileSize / 2;
		// Globals.MapScene.CallDeferred("add_child", spawner);
	}
	
	#region Save/Load
	/******************************************************************
	 * Save/Load                                                      *
	 ******************************************************************/
	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		var dict =  new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", "Map" },
		};
		
		int index = 0;
		foreach (var provence in _provences.Values)
		{
			dict["Provence" + index++] = provence.Save();
		}

		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		int index = 0;
		while (nodeData.ContainsKey("Provence" + index))
		{
			var data = (Godot.Collections.Dictionary<string, Variant>) nodeData["Provence" + index];
			Province.Load(data);
			index++;
		}
	}
	#endregion
}
