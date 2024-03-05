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

	public MapTileMap()
	{
		foreach (var location in GetUsedCells(0))
		{
			var provence = new Province();
			
			_provences.Add(location, provence);

			InitProvence(provence, location == MapGlobals.FactoryPosition ? "Player" : "Enemy");
		}
	}
	
	public override void _Ready()
	{
		base._Ready();
		foreach (var provence in _provences.Values)
		{
			Globals.MapScene.CallDeferred("add_child", provence);
			foreach (var commander in provence.Commanders)
			{
				Globals.MapScene.CallDeferred("add_child", commander);
			}
		}
	}

	private  void InitProvence(Province provence, string team)
	{
		provence.Owner = team;
		if (team == "Player" && Globals.FactoryScene != null) return;
		
		var meleeCommander = new Commander(provence, team);
		meleeCommander.Team = team;
		provence.Commanders.Add(meleeCommander);
		
		var rangedCommander = new Commander(new Province(), "Enemy");
		rangedCommander.Team = team;
		rangedCommander.SpawnLocation = new Vector2I(30, 25);
		provence.Commanders.Add(rangedCommander);
		
		var provinceLocation = _provences.FirstOrDefault(pair => pair.Value == provence).Key;
		var dis = Mathf.Abs(provinceLocation.X - MapGlobals.FactoryPosition.X) + Mathf.Abs(provinceLocation.Y - MapGlobals.FactoryPosition.Y);
		
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
				meleeCommander.Units.Insert("Heavy Infantry", 50);
				rangedCommander.Units.Insert("Archer", 100);
				break;
			case 6:
				meleeCommander.Units.Insert("Elite Infantry", 400);
				meleeCommander.Units.Insert("Barbarian", 400);
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
