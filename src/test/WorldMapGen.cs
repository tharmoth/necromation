using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

public partial class WorldMapGen : Node2D
{
	private const int Width = 1024;
	private const int Height = 1024;
	private const bool ShouldDrawDebug = false;

	public enum Biome
	{
		Ocean,
		Grassland,
	}

	public override void _Ready()
	{
		var map = new VoronoiMap(250, Width, Height);
		var heightTexture = new NoiseTexture2D()
		{
			Noise = new FastNoiseLite()
			{
				Frequency = 1.0f / 375.0f,
				Seed = (int) GD.Randi()
			}, 
			Width = Width, 
			Height = Height, 
			Normalize = true
		};
		heightTexture.Changed += () =>
		{
			var heightMap = heightTexture.GetImage();
			for (var y = 0; y < heightMap.GetHeight(); y++)
			{
				for (var x = 0; x < heightMap.GetWidth(); x++)
				{
					var pixel = new Vector2I(x, y);
					
					var noiseHeight = heightMap.GetPixelv(pixel).R;
					var islandHeight = 1 - DistanceEuclideanSquared(pixel, new Rect2(0, 0, Width, Height));
					// var finalHeight = Mathf.Lerp(noiseHeight, 1 - islandHeight, 1 - islandHeight);
					var finalHeight= (0.5f * noiseHeight + 0.5f * islandHeight);
					heightMap.SetPixelv(new Vector2I(x, y), new Color(finalHeight, finalHeight, finalHeight));
				}
			}
			CalculateBiomes(map, heightMap);
		};
	}
	
	private void CalculateBiomes(VoronoiMap voronoiMap, Image heightMap)
	{
		Dictionary<VoronoiCell, Biome> biomes = new();
		
		foreach (var cell in voronoiMap.Cells.Values)
		{
			var elevation = (heightMap.GetPixelv(cell.Position.ToVector2I()).R - 0.5f) * 2.0f;
			biomes[cell] = elevation < 0.0f ? Biome.Ocean : Biome.Grassland;
		}

		DrawMap(voronoiMap.Cells, biomes);
	}

	private void DrawMap(Dictionary<Vector2, VoronoiCell> cells, Dictionary<VoronoiCell, Biome> biomes)
	{
		var image = Image.Create(Width, Height, true, Image.Format.Rgba8);
		var sprite = new Sprite2D
		{
			Texture = ImageTexture.CreateFromImage(image),
			Centered = false
		};
		CallDeferred("add_child", sprite);
		sprite.Draw += () =>
		{
			foreach (var cell in cells.Values)
			{
				var biomeColor = biomes[cell] == Biome.Ocean ? Colors.Blue : Colors.Green;
				sprite.DrawColoredPolygon(cell.Vertices.ToArray(), biomeColor);

				for (int i = 0; i < cell.Vertices.Count - 1; i++)
				{
					var current = cell.Vertices[i];
					var next = cell.Vertices[i + 1];
					sprite.DrawLine(current, next, Colors.Black);
				}
				

				if (!ShouldDrawDebug) continue;
				
				foreach (var neighbor in cell.Neighbors)
				{
					sprite.DrawLine(cell.Position, neighbor.Position, Colors.Black);
				}
				
				sprite.DrawCircle(cell.Position, 5, Colors.Red);
				foreach (var vertex in cell.Vertices)
				{
					sprite.DrawCircle(vertex, 5, Colors.Blue);
				}
			}
		};
	}
	
		
	private float DistanceEuclideanSquared(Vector2 pos, Rect2 area)
	{
		// pos is a point in the rect area
		// Scale pos such that it is in the range [-1, 1] and the center is (area.x / 2, area.y / 2)
		var scaled = (pos - area.Position) / area.Size * 2 - Vector2.One;
		return 1 - (1.0f - scaled.X * scaled.X) * (1.0f - scaled.Y * scaled.Y);
	}
}
