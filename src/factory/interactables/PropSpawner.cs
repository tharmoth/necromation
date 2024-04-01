using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Necromation;
using Necromation.sk;
using Array = Godot.Collections.Array;

public partial class PropSpawner : Node2D
{
	/**************************************************************************
	 * Input Data           											      *
	 **************************************************************************/
	public readonly Array<Texture2D> Textures = new();
	public int Radius = 1000;
	public int NoiseSeed;
	public int SizePixels = 32;
	public float Threshold = 0.5f;
	public float Density = 0.5f;
	public bool UseWind = true;
	public bool Single = false;

	/**************************************************************************
	 * Instance data        											      *
	 **************************************************************************/
	private readonly List<MultiMesh> _meshes = new();
	private readonly List<CellInfo> _positions = new();

	public override void _Ready()
	{
		base._Ready();
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
			matty.Shader = GD.Load<Shader>("res://src/factory/shaders/wind_sway.gdshader");
			matty.SetShaderParameter("minStrength", 0.00175f);
			matty.SetShaderParameter("maxStrength", 0.0025f);
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

		for (var x = -Radius; x < Radius; x += 32)
		{
			for (var y = -Radius; y < Radius; y += 32)
			{
				var position = new Vector2(x, y);
				var noise = (Utils.NoiseNorm(smallNoise, position) + Utils.NoiseNorm(bigNoise, position)) / 2.0;
				if (noise < Threshold) continue;
				FillCell(new Vector2(x, y), noise);
			}
		}
	}
	
	private void FillCell(Vector2 position, double noise)
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
		for (var x = 5; x < 32; x += 10)
		{
			for (var y = 5; y < 32; y += 10)
			{
				if (!(GD.Randf() < Density)) continue;
				var pos = position + new Vector2(x + GD.RandRange(-3, 3), y + GD.RandRange(-3, 3));
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
