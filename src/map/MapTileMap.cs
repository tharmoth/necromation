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
	
	/**************************************************************************
	 * State Variables                                                        *
	 **************************************************************************/
	private readonly Dictionary<Vector2I, Province> _provences = new();
	
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	public const int TileSize = 32;

	public MapTileMap()
	{
		foreach (var location in GetUsedCells(0))
		{
			var provence = new Province();
			_provences.Add(location, provence);
		}
	}
	
	public override void _Ready()
	{
		base._Ready();
		foreach (var (location, provence) in _provences)
		{
			provence.Init(location == MapScene.FactoryPosition ? "Player" : "Enemy");

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
