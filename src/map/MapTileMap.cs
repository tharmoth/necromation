using Godot;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class MapTileMap : SKTileMap
{
	/**************************************************************************
	 * Utility Property                                                       *
	 **************************************************************************/
	public List<Province> Provinces => _provences.Values.ToList();
	private ProvinceBorders ProvinceBorders => GetNode<ProvinceBorders>("%ProvinceBorders");
	
	/**************************************************************************
	 * State Variables                                                        *
	 **************************************************************************/
	private readonly Dictionary<Vector2I, Province> _provences = new();
	private readonly Dictionary<Vector2I, Node2D> _fogs = new();
	
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	public const int TileSize = 128;
	
	public override void _Ready()
	{
		base._Ready();
		
		foreach (var location in GetUsedCells(0))
		{
			var provence = new Province(location);
			_provences.Add(location, provence);
			
			var team = location == MapScene.FactoryPosition ? "Player" : "Enemy";
			provence.Owner = team;

			InitCommanders(provence);
			Globals.MapScene.CallDeferred("add_child", provence);
		}
		
		foreach (var province in Globals.MapScene.TileMap.Provinces)
		{
			AddFog(province.MapPosition);
			SpawnGrass(province.MapPosition);
		}

		for (int x = -20; x < 20; x++)
		{
			for (int y = -20; y < 20; y++)
			{
				var position = new Vector2I(x, y);
				if (_fogs.ContainsKey(position)) continue;
				AddFog(new Vector2I(x, y));
			}
		}

		UpdateFogOfWar();
	}


	public void UpdateFogOfWar()
	{
		_provences.Keys
			.Where(mapPos => _fogs.ContainsKey(mapPos))
			.Select(mapPos => _fogs[mapPos])
			.ToList()
			.ForEach(fog => fog.Visible = true);
		_provences.Values
			.ToList()
			.ForEach(province => province.Visible = false);
		_provences.Values
			.ToList()
			.SelectMany(province => province.Commanders)
			.ToList()
			.ForEach(commander => commander.Visible = false);
		
		_provences.Values.Where(province => province.Owner == "Player")
			.SelectMany(province =>
			{
				var visible = new List<Vector2I>
				{
					province.MapPosition,
					province.MapPosition + Vector2I.Up,
					province.MapPosition + Vector2I.Down,
					province.MapPosition + Vector2I.Left,
					province.MapPosition + Vector2I.Right
				};
				return visible;
			})
			.Distinct()
			.Where(mapPos => _provences.ContainsKey(mapPos))
			.Select(mapPos => _provences[mapPos])
			.ToList()
			.ForEach(province =>
			{
				if (_fogs.TryGetValue(province.MapPosition, out var fog)) fog.Visible = false;
				province.Visible = true;
				province.Commanders.ForEach(commander => commander.Visible = true);
			});
		
		ProvinceBorders.UpdateBorders();
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
        rangedCommander.TargetType = Commander.Target.Random;
		
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
		var startpos = (location) * TileSize * (1 + 0);
		
		Sprite2D sprite = new();
		sprite.Texture = Database.Instance.GetTexture("fow2");
		var scaler = (TileSize * 1) / (sprite.Texture.GetSize().X - 192);
		sprite.Scale = new Vector2(scaler, scaler);
		sprite.GlobalPosition = startpos + scaler * sprite.Texture.GetSize() / 2 - scaler * Vector2.One * 96;
		sprite.RotationDegrees = new List<float> { 0, 90, 180, 270 }[GD.RandRange(0, 3)];
		sprite.Centered = true;
		sprite.ZIndex = -1;
		CanvasItemMaterial material = new();
		material.BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha;
		sprite.Material = material;
		Globals.MapScene.CallDeferred("add_child", sprite);
		_fogs.Add(location, sprite);
	}
	
	private void SpawnGrass(Vector2I location)
	{
		var startpos = (location) * TileSize * (1 + 0);
		
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
		
		Sprite2D grassSprite = new(); 
		grassSprite.Texture = Database.Instance.GetTexture("Grass");
		var scaler2 = (TileSize * 1) / (grassSprite.Texture.GetSize().X - 32);
		grassSprite.Scale = new Vector2(scaler2, scaler2);
		grassSprite.GlobalPosition = startpos + scaler2 * grassSprite.Texture.GetSize() / 2 - scaler2 * Vector2.One * 16;
		grassSprite.Centered = true;
		grassSprite.ZIndex = -99;
		Globals.MapScene.CallDeferred("add_child", grassSprite);
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
