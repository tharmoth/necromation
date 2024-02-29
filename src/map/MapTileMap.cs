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

			InitProvence(provence, location == MapGlobals.FactoryPosition ? "Player" : "Enemy");
		}
	}

	private  void InitProvence(Province provence, string team)
	{
		provence.Owner = team;
		if (team == "Player" && Globals.FactoryScene != null) return;
		
		var meleeCommander = new Commander(provence, team);
		meleeCommander.Team = team;
		provence.Commanders.Add(meleeCommander);
		Globals.MapScene.CallDeferred("add_child", meleeCommander);
		
		var rangedCommander = new Commander(new Province(), "Enemy");
		rangedCommander.Team = team;
		rangedCommander.SpawnLocation = new Vector2I(30, 25);
		provence.Commanders.Add(rangedCommander);
		Globals.MapScene.CallDeferred("add_child", rangedCommander);

		var provinceLocation = _provences.FirstOrDefault(pair => pair.Value == provence).Key;
		switch (provinceLocation.X)
		{
			case 0:
				meleeCommander.Units.Insert("Rabble", 100);
				break;
			case 1:
				meleeCommander.Units.Insert("Light Infantry", 100);
				break;
			case 2:
				meleeCommander.Units.Insert("Infantry", 100);
				rangedCommander.Units.Insert("Archer", 100);
				break;
			case 3:
				meleeCommander.Units.Insert("Heavy Infantry", 50);
				meleeCommander.Units.Insert("Infantry", 200);
				rangedCommander.Units.Insert("Archer", 100);
				break;
			case 4:
				meleeCommander.Units.Insert("Barbarian", 400);
				break;
			case 5:
				meleeCommander.Units.Insert("Heavy Infantry", 200);
				rangedCommander.Units.Insert("Archer", 100);
				break;
			case 6:
				meleeCommander.Units.Insert("Heavy Infantry", 400);
				rangedCommander.Units.Insert("Archer", 100);
				break;
			case 7:
				meleeCommander.Units.Insert("Heavy Infantry", 500);
				rangedCommander.Units.Insert("Archer", 250);
				break;
			case 8:
				meleeCommander.Units.Insert("Elite Infantry", 1000);
				rangedCommander.Units.Insert("Archer", 1000);
				break;
		}
		
		if (team == "Player")
		{
			meleeCommander.Units.Clear();
			meleeCommander.Units.Insert("Elite Infantry", 1000);
		}
	}

	public void MoveUnitsToFromFactory()
	{
		if (Globals.TileMap == null) return;
		
		var prov = _provences[Vector2I.One];
		foreach (var barracks in Globals.TileMap.GetEntitiesOfType(nameof(Barracks)).OfType<Barracks>())
		{
			var inventory = barracks.GetInventories().First();
			var count = inventory.CountItem("Infantry");
			prov.Units.Insert("Infantry", count);
			inventory.Remove("Infantry", count);
		}
	}

	public Province GetProvence(Vector2I position)
	{
		return _provences.TryGetValue(position, out var provence) ? provence : null;
	}
	
	public List<Province> GetProvinces()
	{
		return _provences.Values.ToList();
	}
	
	public Vector2I GetLocation(Province provence)
	{
		return _provences.FirstOrDefault(pair => pair.Value == provence).Key;
	}
}
