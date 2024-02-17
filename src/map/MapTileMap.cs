using Godot;
using System.Collections.Generic;
using System.Linq;
using Necromation.map;
using Necromation.map.character;

public partial class MapTileMap : SKTileMap
{
	private readonly Dictionary<Vector2I, Province> _provences = new();
	
	public override void _EnterTree()
	{
		MapGlobals.TileMap = this;
	}

	public override void _Ready()
	{
		base._Ready();
		foreach (var location in GetUsedCells(0))
		{
			var provence = new Province();
			_provences.Add(location, provence);
			GetTree().Root.CallDeferred("add_child", provence);
		}
		var prov = _provences[Vector2I.One];

		prov.Units.Insert("soldier", 10);
		prov.Units.Insert("archer", 10);
		prov.Units.Insert("horse", 10);
		
		var commander = new Commander(prov);
		prov.Commanders.Add(commander);
		commander.Units.Insert("warrior", 10);
		GetTree().Root.CallDeferred("add_child", commander);

	}

	public Province GetProvence(Vector2I position)
	{
		return _provences.TryGetValue(position, out var provence) ? provence : null;
	}
	
	public Vector2I GetLocation(Province provence)
	{
		return _provences.FirstOrDefault(pair => pair.Value == provence).Key;
	}
}
