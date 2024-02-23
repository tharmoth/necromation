using Godot;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class MapTileMap : SKTileMap
{
	private readonly Dictionary<Vector2I, Province> _provences = new();
	public const int TileSize = 32;
	
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
			Globals.MapScene.CallDeferred("add_child", provence);

			InitProvence(provence, location == Vector2I.One ? "Player" : "Enemy");
		}
	}

	private  void InitProvence(Province provence, string team)
	{
		provence.Owner = team;
		var commander = new Commander(provence)
		{
			Team = team
		};
		provence.Commanders.Add(commander);
		// Get the location of the provence
		var pos = _provences.FirstOrDefault(pair => pair.Value == provence).Key;
		// for each unit distance away from (1, 1) add 10 warriors to the commander.
		var distance = (pos - Vector2I.One).Length();
		if (team == "Player") distance = 2;
		commander.Units.Insert("Warrior", (int)distance * 10);
		Globals.MapScene.CallDeferred("add_child", commander);
	}

	public void MoveUnitsToFromFactory()
	{
		if (Globals.TileMap == null) return;
		
		var prov = _provences[Vector2I.One];
		foreach (var barracks in Globals.TileMap.GetEntitiesOfType(nameof(Barracks)).OfType<Barracks>())
		{
			var inventory = barracks.GetInventories().First();
			var count = inventory.CountItem("Warrior");
			prov.Units.Insert("Warrior", count);
			inventory.Remove("Warrior", count);
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
}
