using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.sk;

public partial class BattleTileMap : LayerTileMap
{
	public const string Unit = "unit";
	public static Vector2I TileSize = new(32, 32);
	
	private readonly AStar2D _grid = new();
	private Dictionary<Vector2I, int> cells = new();
	
	private const int X = 100;
	private const int Y = 100;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Globals.BattleScene.TileMap = this;
		AddLayer(Unit);
		
		

		var index = 0;
		for (int x = 0; x < X; x++)
		{ 
			for (int y = 0; y < Y; y++)
			{
				var coords = new Vector2I(x, y);
				
				// GD.Print(GetCellAlternativeTile(0));
				
				// get a random vector between 0, 0 and 7, 3

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
				

				SetCell(0, coords, 1, randomvec);
				_grid.AddPoint(index, coords);
				cells.Add(coords, index);
				index++;
			}
		}
		
		for (int x = 0; x < X; x++)
		{
			for (int y = 0; y < Y; y++)
			{
				var up = new Vector2I(x, y - 1);
				var down = new Vector2I(x, y + 1);
				var left = new Vector2I(x - 1, y);
				var right = new Vector2I(x + 1, y);

				Connect(new Vector2I(x, y), up);
				Connect(new Vector2I(x, y), down);
				Connect(new Vector2I(x, y), left);
				Connect(new Vector2I(x, y), right);
			}
		}
	}

	private void Connect(Vector2I a, Vector2I b)
	{
		if (!cells.ContainsKey(a) || !cells.ContainsKey(b)) return;
		_grid.ConnectPoints(cells[a], cells[b]);
	}

	public override bool AddEntity(Vector2I position, IEntity entity, string layerName)
	{
		if (!base.AddEntity(position, entity, layerName)) return false;
		_grid.SetPointDisabled(cells[position], true);
		return true;
	}
	
	public override bool RemoveEntity(Vector2I position, string layerName)
	{
		if(!base.RemoveEntity(position, layerName)) return false;
		_grid.SetPointDisabled(cells[position], false);
		return true;
	}

	public Vector2I GetNextPath(Vector2I mapFrom, Vector2I mapTo)
	{
		var disabled = _grid.IsPointDisabled(cells[mapTo]);
		_grid.SetPointDisabled(cells[mapFrom], false);
		_grid.SetPointDisabled(cells[mapTo], false);
		var path = _grid.GetPointPath(cells[mapFrom], cells[mapTo]);
		_grid.SetPointDisabled(cells[mapFrom], true);
		_grid.SetPointDisabled(cells[mapTo], disabled);
		return path.Length < 2 ?  mapFrom : (Vector2I) path[1];
	}

	public Vector2I GetNearestEmpty(Vector2I to)
	{
		if (!_grid.IsPointDisabled(cells[to])) return to;
		// Do this by scanning in a spiral pattern over the grid
		var radius = 1;
		while (radius < 50)
		{
			for (var x = -radius; x <= radius; x++)
			{
				for (var y = -radius; y <= radius; y++)
				{
					if (Math.Abs(x) + Math.Abs(y) != radius) continue;
					var testPoint = to + new Vector2I(x, y);
					
					if (!cells.ContainsKey(testPoint)) continue;
					if (_grid.IsPointDisabled(cells[testPoint])) continue;
					// Too performance intensive we'll have to rethink this.
					// if (_grid.GetPointPath(cells[from], cells[testPoint]).IsEmpty()) continue;
					return testPoint;
				}
			}
			radius++;
		}

		GD.PrintErr("Failed to find near empty cell!");
		return Vector2I.Zero;
	}

	public List<Unit> GetUnitsInRange(Vector2I mapCenter, float mapRadius)
	{
		return GetTilesInRadius(mapCenter, mapRadius)
			.Select(tile => GetEntity(tile, Unit))
			.OfType<Unit>()
			.ToList();
	}
	
	public bool IsSurrounded(Vector2I mapPos)
	{
		return Globals.BattleScene.TileMap.GetUnitsInRange(mapPos, 1).Count == 5;
	}
 }
