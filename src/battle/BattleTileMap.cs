using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Necromation;
using Necromation.sk;

public partial class BattleTileMap : LayerTileMap
{
	public const string Unit = "unit";
	public const int TileSize = 32;
	
	private readonly AStar2D _grid = new();
	private System.Collections.Generic.Dictionary<Vector2I, int> _cells = new();
	
	public const int X = 200;
	public const int Y = 150;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		AddLayer(Unit);

		var index = 0;
		for (int x = 0; x < X; x++)
		{ 
			for (int y = 0; y < Y; y++)
			{
				var coords = new Vector2I(x, y);
				_grid.AddPoint(index, coords);
				_cells.Add(coords, index);
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
		
		for (int x = 0; x < X * TileSize; x += FactoryTileMap.TileSize * FactoryTileMap.ProvinceSize)
		{
			for (int y = 0; y < Y * TileSize; y  += FactoryTileMap.TileSize * FactoryTileMap.ProvinceSize)
			{
				var startpos = new Vector2(x, y);
				
				Sprite2D sprite = new();
				sprite.Texture = Database.Instance.GetTexture("soil2");
				var scaler2 = (FactoryTileMap.TileSize * FactoryTileMap.ProvinceSize) / sprite.Texture.GetSize().X;
				sprite.Scale = new Vector2(scaler2, scaler2);
				sprite.GlobalPosition = startpos + scaler2 * sprite.Texture.GetSize() / 2;
				sprite.Centered = true;
				sprite.ZIndex = -99;
				Globals.BattleScene.CallDeferred("add_child", sprite);

				PropSpawner spawner = new();
				spawner.Textures.Add(Database.Instance.GetTexture("Grass2"));
				spawner.Textures.Add(Database.Instance.GetTexture("Grass5"));
				spawner.Density = 0.8f;
				spawner.Radius = FactoryTileMap.ProvinceSize * TileSize / 2;
				spawner.SizePixels = 24;
				spawner.Threshold = 0.4f;
				spawner.GlobalPosition = startpos + Vector2I.One * FactoryTileMap.ProvinceSize * TileSize / 2;
				spawner.ZIndex = -98;
				Globals.BattleScene.GetNode<Node2D>("GrassHolder").CallDeferred("add_child", spawner);
			}
		}
	}

	private void Connect(Vector2I a, Vector2I b)
	{
		if (!_cells.ContainsKey(a) || !_cells.ContainsKey(b)) return;
		_grid.ConnectPoints(_cells[a], _cells[b]);
	}

	public override bool AddEntity(Vector2I position, IEntity entity, string layerName)
	{
		if (!base.AddEntity(position, entity, layerName)) return false;
		_grid.SetPointDisabled(_cells[position], true);
		return true;
	}
	
	public override bool RemoveEntity(Vector2I position, string layerName)
	{
		if(!base.RemoveEntity(position, layerName)) return false;
		_grid.SetPointDisabled(_cells[position], false);
		return true;
	}

	public Vector2I GetNextPath(Vector2I mapFrom, Vector2I mapTo)
	{
		var disabled = _grid.IsPointDisabled(_cells[mapTo]);
		_grid.SetPointDisabled(_cells[mapFrom], false);
		_grid.SetPointDisabled(_cells[mapTo], false);
		var path = _grid.GetPointPath(_cells[mapFrom], _cells[mapTo]);
		_grid.SetPointDisabled(_cells[mapFrom], true);
		_grid.SetPointDisabled(_cells[mapTo], disabled);
		return path.Length < 2 ?  mapFrom : (Vector2I) path[1];
	}

	public Vector2I GetNearestEmpty(Vector2I to)
	{
		if (_cells.ContainsKey(to) && !_grid.IsPointDisabled(_cells[to])) return to;
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
					
					if (!_cells.ContainsKey(testPoint)) continue;
					if (_grid.IsPointDisabled(_cells[testPoint])) continue;
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
	
	public override bool IsOnMap(Vector2I mapPos)
	{
		return _cells.ContainsKey(mapPos);
	}
 }
