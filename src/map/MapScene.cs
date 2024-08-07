using System;
using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.bridges;
using Necromation.map;
using Necromation.map.character;

public partial class MapScene : Scene, Locator.IMap
{
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	public Vector2I GRASS => new(0, 0);
	public Vector2I DIRT => new(1, 0);
	public Vector2I WATER => new(2, 0);
	public Vector2I SAND => new(3, 0);
	public Vector2I STONE => new(4, 0);
	
	public static Vector2I FactoryPosition => new(4, 2);
	public static float MapCellSize = 64.0f;
	
	/**************************************************************************
	 * Public Variables                                                       *
	 **************************************************************************/
	public override MapGui Gui => GetNode<MapGui>("%GUI");
	public readonly List<Action> UpdateListeners = new();
	public readonly List<Actor> Actors = [];
	public readonly WorldMap Map;
	public Actor Player => Actors.First(a => a.GetComponent<StatsComponent>().Team == "Player");
	public List<Commander> PlayerCommanders => Player.GetComponent<ArmyComponent>().Commanders;
	
	/**************************************************************************
	 * Constructor                                                            *
	 **************************************************************************/
	public MapScene()
	{
		Map = new WorldMap(10.0f);
		
		var tileMap = Database.GetScene("MapTileMap").Instantiate<TileMap>();
		AddChild(tileMap);
		
		Map.Provinces.ForEach(provence => CallDeferred("add_child", provence));
		Map.Provinces.Where(provence => provence.Biome != Biome.Ocean).RandomElement().Owner = "Player";

		Locator.Map = this;

		// for (int y = 0; y < 160; y++)
		// {
		// 	for (int x = 0; x < 160; x++)
		// 	{
		// 		var mapPosition = new Vector2I(x, y);
		// 		
		// 		var cellId = Map.GetProvenceGlobal(tileMap.MapToGlobal(mapPosition)).Biome switch
		// 		{
		// 			Biome.Grassland => GRASS,
		// 			Biome.Ocean => WATER,
		// 			_ => throw new ArgumentOutOfRangeException()
		// 		};
		// 		tileMap.SetCell(0, mapPosition, 0, cellId);
		// 	}
		// }
	}
	
	/**************************************************************************
	 * Godot Methods                                                          *
	 **************************************************************************/
	public override void _Ready()
	{
		var actor = Actor.ActorBuilder.Create().AsPlayer().Build();
		CallDeferred("add_child", actor);
		Actors.Add(actor);
		
		// var enemy = Actor.ActorBuilder.Create().AsEnemy().Build();
		// CallDeferred("add_child", enemy);
		// Actors.Add(enemy);
		
		var cursor = new CursorIconManager();
		CallDeferred("add_child", cursor);
	}
	
	/**************************************************************************
	 * Public Methods                                                         *
	 **************************************************************************/
	public override void OnOpen()
	{
		Camera.Position = Actors.First(actor => actor.GetComponent<StatsComponent>().Team == "Player").GlobalPosition;
	}

	public override void OnClose() {}
	
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
		foreach (var provence in Map.Provinces)
		{
			dict["Provence" + index++] = provence.Save();
		}

		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		Globals.MapScene.Map.Provinces.ForEach(province => province.QueueFree());
		Globals.MapScene.Map.Provinces.Clear();
		
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
