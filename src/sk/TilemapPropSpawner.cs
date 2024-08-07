
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

public partial class TilemapPropSpawner : Node2D
{
	/**************************************************************************
	 * Static Imports        											      *
	 **************************************************************************/
	private static readonly Shader WindSway = GD.Load<Shader>("res://src/factory/shaders/wind_sway.gdshader");
	
    /**************************************************************************
	 * Input Data           											      *
	 **************************************************************************/
    /// <summary>
    /// List of Texture2D to spawn. needs at least one texture.
    /// </summary>
    public required ImmutableList<Texture2D> Textures { get; init; }
    /// <summary>
    /// List of coordinates in the atlas to spawn props on.
    /// </summary>
    public required ImmutableList<Vector2I> AtlasCoords { get; init; }
    /// <summary>
    /// Tilemap to spawn props on
    /// </summary>
    public required TileMap TargetTileMap { get; init; }
    /// <summary>
    /// Used to generate noise values for prop placement
    /// </summary>
    public int NoiseSeed;
	/// <summary>
	/// Size of each prop in pixels
	/// </summary>
	public int SizePixels = 32;
	/// <summary>
	/// Percent chance a cell will contain props
	/// </summary>
	public float Threshold = 0.5f;
	/// <summary>
	/// Percent chance a prop will be spawned in a slot in a cell
	/// </summary>
	public float Density = 0.5f;
	/// <summary>
	/// Should the spawned props sway in the wind.
	/// </summary>
	public bool UseWind = true;
	/// <summary>
	/// Should the spawner only spawn one prop per cell.
	/// </summary>
	public bool Single = false;

	/**************************************************************************
	 * Instance data        											      *
	 **************************************************************************/
	private readonly List<MultiMesh> _meshes = [];
	private readonly List<CellInfo> _positions = [];

	public override void _Ready()
	{
		CallDeferred("Spawn");
	}

	private void Spawn()
	{
		foreach (var texture in Textures)
		{
			GenerateMultiMesh(texture);
		}

		CalculatePositions();

		foreach (var multiMesh in _meshes)
		{
			var positions = _positions
				.Where(pos => pos.TextureIndex == _meshes.IndexOf(multiMesh))
				.ToList();
			
			multiMesh.InstanceCount = positions.Count;
			foreach (var (index, position) in positions.Select(pos => pos.Position).Select((value, index) => (Index: index, Value: value)))
			{
				var transform = Transform2D.Identity
					.Translated(position)
					.RotatedLocal(Mathf.Pi + Mathf.DegToRad(new Random().Next(-10, 10)))
					.ScaledLocal(Vector2.One * SizePixels * (float)GD.RandRange(.5f, 1.5f));
				multiMesh.SetInstanceTransform2D(index, transform);
			}
		}
	}

	private void GenerateMultiMesh(Texture2D texture)
	{
		MultiMesh multiMesh = new(); 
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;
		multiMesh.Mesh = new QuadMesh();
		multiMesh.InstanceCount = 0;
		_meshes.Add(multiMesh);
		
		MultiMeshInstance2D multiMeshInstance = new(); 
		multiMeshInstance.Multimesh = multiMesh;
		multiMeshInstance.Texture = texture;
		if (UseWind)
		{
			ShaderMaterial matty = new();
			matty.Shader = WindSway;
			matty.SetShaderParameter("minStrength", 1.0f/1200.0f);
			matty.SetShaderParameter("maxStrength", 1.0f/800.0f);
			matty.SetShaderParameter("detail", 5.0f);
			multiMeshInstance.Material = matty;
		}
		AddChild(multiMeshInstance);
	}
	
	private class CellInfo
	{
		public Vector2 Position;
		public int TextureIndex;
	}
	
	/// <summary>
	/// Calculates the position to place props and stores the data in _positions. Positions are based on noise values.
	/// </summary>
	private void CalculatePositions()
	{
		var smallNoise = new FastNoiseLite();
		smallNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		smallNoise.Seed = NoiseSeed;
		smallNoise.Frequency = 0.005f;
		smallNoise.Offset = new Vector3(GlobalPosition.X, GlobalPosition.Y, 0);
		
		var bigNoise = new FastNoiseLite();
		bigNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		bigNoise.Seed = NoiseSeed;
		bigNoise.Frequency = 0.0005f;
		bigNoise.Offset = new Vector3(GlobalPosition.X, GlobalPosition.Y, 0);
		
		var rect = TargetTileMap.GetUsedRect();
		for (var y = rect.Position.Y; y < rect.End.Y; y++)
		{
			for (var x = rect.Position.X; x < rect.End.X; x++)
			{
				var cell = TargetTileMap.GetCellAtlasCoords(0, new Vector2I(x, y));
				if (!AtlasCoords.Contains(cell)) continue;
				
				var position = new Vector2(x * TargetTileMap.TileSet.TileSize.X, y * TargetTileMap.TileSet.TileSize.Y);
				var noise = (Utils.NoiseNorm(smallNoise, position) + Utils.NoiseNorm(bigNoise, position)) / 2.0;
				
				if (noise > Threshold) continue;
				FillCell(position);
			}
		}
	}
	
	/// <summary>
	/// Fills a cell with props based on density
	/// </summary>
	/// <param name="position">top left corner of the cell</param>
	private void FillCell(Vector2 position)
	{
		if (Single)
		{
			CellInfo cellInfo = new()
			{
				Position = position + new Vector2(GD.RandRange(-16, 16), GD.RandRange(-16, 16)),
				TextureIndex = new Random().Next(0, Textures.Count)
			};
			_positions.Add(cellInfo);
			return;
		}
		
		var xStep = TargetTileMap.TileSet.TileSize.X / 3;
		var yStep = TargetTileMap.TileSet.TileSize.Y / 3;
		
		for (var x = 5; x < TargetTileMap.TileSet.TileSize.X; x += xStep)
		{
			for (var y = 5; y < TargetTileMap.TileSet.TileSize.Y; y += yStep)
			{
				if (GD.Randf() > Density) continue;
				var pos = position + new Vector2(x + GD.RandRange(-xStep, xStep), y + GD.RandRange(-yStep, yStep));
				CellInfo cellInfo = new()
				{
					Position = pos,
					TextureIndex = new Random().Next(0, Textures.Count)
				};
				_positions.Add(cellInfo);
			}
		}
	}

}