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

	public MapTileMap()
	{
		foreach (var location in GetUsedCells(0))
		{
			GD.Print(GetCellSourceId(0, location));
			if (GetCellSourceId(0, location) != 0) continue;
			var provence = new Province(GetResource(location));
			_provences.Add(location, provence);
		}
	}

	public void OnOpen()
	{
		_provences.Values.ToList().ForEach(province => province.OnOpen());
	}
	
	public override void _Ready()
	{
		base._Ready();

		foreach (var location in GetUsedCells(0))
		{
			if (GetCellSourceId(0, location) != 0) continue;
			var provence = _provences[location];
			
			var coords = GetCellAtlasCoords(0, location);
			var team = coords.Equals(new Vector2I(2, 1)) ? "Player" : "Enemy";

			provence.Owner = team;

			InitCommanders(provence);
			Globals.MapScene.CallDeferred("add_child", provence);
		}
		
		foreach (var location in GetUsedCells(0))
		{
			if (GetCellSourceId(0, location) == 1) continue;
			SpawnGrass(location);
		}
		
		foreach (var province in Globals.MapScene.TileMap.Provinces)
		{
			AddFog(province.MapPosition);
		}

		// for (int x = -20; x < 20; x++)
		// {
		// 	for (int y = -20; y < 20; y++)
		// 	{
		// 		var position = new Vector2I(x, y);
		// 		if (_fogs.ContainsKey(position)) continue;
		// 		AddFog(new Vector2I(x, y));
		// 	}
		// }

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
	private void InitCommanders(Province province)
	{
        if (province.Owner == "Player" && Globals.FactoryScene != null) return;
		
        var meleeCommander = new Commander(province, province.Owner);
        meleeCommander.SpawnLocation = new Vector2I(90, 50);
        var meleeCommander2 = new Commander(province, province.Owner);
        meleeCommander2.SpawnLocation = new Vector2I(90, 25);
        var meleeCommander3 = new Commander(province, province.Owner);
        meleeCommander3.SpawnLocation = new Vector2I(90, 75);
        var rangedCommander = new Commander(province, province.Owner);
        rangedCommander.SpawnLocation = new Vector2I(70, 50);
        rangedCommander.TargetType = Commander.Target.Random;
        var dragon = new Commander(province, province.Owner);
        dragon.SpawnLocation = new Vector2I(80, 50);
        dragon.TargetType = Commander.Target.Random;
		
        var coords = GetCellAtlasCoords(0, province.MapPosition);
        if (coords.Equals(new Vector2I(0, 0)))
		{
	        meleeCommander.Units.Insert("Rabble", GD.RandRange(25, 40));
		}
        if (coords.Equals(new Vector2I(1, 0)))
        {
	        meleeCommander.Units.Insert("Rabble", GD.RandRange(50, 150));
	        if (meleeCommander.Units.CountItems() < 100)
	        {
		        rangedCommander.Units.Insert("Archer", GD.RandRange(10, 20));
	        }
        }
        if (coords.Equals(new Vector2I(2, 0)))
        {
	        var rand = GD.Randf();
	        if (rand > .5)
	        {
		        meleeCommander.Units.Insert("Infantry", GD.RandRange(25, 50));
		        meleeCommander2.Units.Insert("Rabble", GD.RandRange(50, 100));
		        meleeCommander3.Units.Insert("Rabble", GD.RandRange(50, 10));
		        rangedCommander.Units.Insert("Archer", GD.RandRange(25, 50));
	        }
	        else
	        {
		        meleeCommander.Units.Insert("Infantry", GD.RandRange(25, 35));
		        meleeCommander2.Units.Insert("Infantry", GD.RandRange(25, 35));
		        meleeCommander3.Units.Insert("Infantry", GD.RandRange(25, 35));
		        rangedCommander.Units.Insert("Archer", GD.RandRange(75, 150));
	        }
        }
        if (coords.Equals(new Vector2I(3, 0)))
        {
	        var rand = GD.Randf();
	        if (rand > .5)
	        {
		        meleeCommander.Units.Insert("Heavy Infantry", GD.RandRange(50, 100));
		        meleeCommander2.Units.Insert("Infantry", GD.RandRange(25, 50));
		        meleeCommander3.Units.Insert("Infantry", GD.RandRange(25, 50));
		        rangedCommander.Units.Insert("Archer", GD.RandRange(25, 50));
	        } 
	        else if (rand > .25)
	        {
		        meleeCommander.Units.Insert("Infantry", GD.RandRange(50, 100));
		        meleeCommander2.Units.Insert("Infantry", GD.RandRange(25, 50));
		        meleeCommander3.Units.Insert("Infantry", GD.RandRange(25, 50));
		        rangedCommander.Units.Insert("Archer", GD.RandRange(50, 100));
	        }
	        else
	        {
		        meleeCommander.Units.Insert("Heavy Infantry", GD.RandRange(25, 35));
		        meleeCommander2.Units.Insert("Heavy Infantry", GD.RandRange(25, 35));
		        meleeCommander3.Units.Insert("Heavy Infantry", GD.RandRange(25, 35));
		        rangedCommander.Units.Insert("Archer", GD.RandRange(100, 200));
	        }
        }
        if (coords.Equals(new Vector2I(0, 1)))
		{
			var rand = GD.Randf();
			if (rand > .5)
			{
				meleeCommander.Units.Insert("Elite Infantry", GD.RandRange(50, 100));
				meleeCommander2.Units.Insert("Infantry", GD.RandRange(25, 50));
				meleeCommander3.Units.Insert("Infantry", GD.RandRange(25, 50));
				rangedCommander.Units.Insert("Archer", GD.RandRange(25, 50));
			} 
			else if (rand > .25)
			{
				meleeCommander.Units.Insert("Infantry", GD.RandRange(50, 100));
				meleeCommander2.Units.Insert("Infantry", GD.RandRange(25, 50));
				meleeCommander3.Units.Insert("Infantry", GD.RandRange(25, 50));
				rangedCommander.Units.Insert("Archer", GD.RandRange(50, 100));
			}
			else
			{
				meleeCommander.Units.Insert("Heavy Infantry", GD.RandRange(25, 35));
				meleeCommander2.Units.Insert("Elite Infantry", GD.RandRange(50, 100));
				meleeCommander3.Units.Insert("Heavy Infantry", GD.RandRange(25, 35));
				rangedCommander.Units.Insert("Archer", GD.RandRange(100, 200));
			}
		}

        if (coords.Equals(new Vector2I(0, 3)))
        {
	        meleeCommander.Units.Insert("Infantry", 200);
	        meleeCommander2.Units.Insert("Elite Infantry", 200);
	        meleeCommander3.Units.Insert("Infantry", 200);
	        rangedCommander.Units.Insert("Archer", GD.RandRange(100, 200));
	        dragon.Units.Insert("Dragon", 1);
        }
        
	}

	public string GetResource(Vector2I mapPos)
	{
		var coords = GetCellAtlasCoords(1, mapPos);
		if (coords.Equals(new Vector2I(0, 0)))
		{
			return "Coal Ore";
		}
		if (coords.Equals(new Vector2I(1, 1)))
		{
			return "Stone";
		}
		if (coords.Equals(new Vector2I(1, 2)))
		{
			return "Copper Ore";
		}
		if (coords.Equals(new Vector2I(3, 1)))
		{
			return "Tin Ore";
		}
		if (coords.Equals(new Vector2I(3, 3)))
		{
			return "Bone Fragments";
		} 
		
		return "";
	}

	public List<Vector2I> GetOcean()
	{
		return GetUsedCells(0).Where(location => GetCellSourceId(0, location) == 1).ToList();
	}
	
	public List<Vector2I> GetMountain()
	{
		return GetUsedCells(0).Where(location => GetCellSourceId(0, location) == 2).ToList();
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
