using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.sk;

public partial class BattleTileMap : LayerTileMap
{
	public const string Unit = "unit";
	
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
				SetCell(0, coords, 0, Vector2I.Zero, 0);
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
		_grid.SetPointDisabled(cells[mapFrom], false);
		var path = _grid.GetPointPath(cells[mapFrom], cells[mapTo]);
		if (!path.IsEmpty()) return path.Length < 2 ?  mapFrom : (Vector2I) path[1];
		
		mapTo = GetNearestEmpty(mapTo);
		path = _grid.GetPointPath(cells[mapFrom], cells[mapTo]);
		_grid.SetPointDisabled(cells[mapFrom], true);
		return path.Length < 2 ?  mapFrom : (Vector2I) path[1];
	}

	public Vector2I GetNearestEmpty(Vector2I to)
	{
		if (!_grid.IsPointDisabled(cells[to])) return to;
		// Perform a breadth-first search to find the nearest empty cell
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
}
