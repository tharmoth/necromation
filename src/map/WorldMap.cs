using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Necromation.map;

public enum Biome
{
	Ocean,
	Grassland,
}

/// <summary>
/// Annotates a Voronoi Map with game state information.
/// </summary>
public class WorldMap
{
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
    private const int UnscaledWidth = 1024;
    private const int UnscaledHeight = 1024;
    private const bool ShouldDrawDebug = false;
    
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    public readonly List<Province> Provinces = [];
   
    /**************************************************************************
	 * Private Variables                                                      *
	 **************************************************************************/
    private readonly float _scale;
    
    /**************************************************************************
     * Constructor                                                            *
     **************************************************************************/
    public WorldMap(float scale = 1.0f)
    {
	    _scale = scale;
	    
    	var map = new VoronoiMap(250, Mathf.FloorToInt(UnscaledWidth), Mathf.FloorToInt(UnscaledHeight));
	    
	    var noisy = new FastNoiseLite()
	    {
		    Frequency = 1.0f / 375.0f,
		    Seed = (int)GD.Randi()
	    };
	    var heightMap = Image.Create(UnscaledWidth, UnscaledHeight, false, Image.Format.Rgb8);
	    var max = float.MinValue;
	    var min = float.MaxValue;
	    for (var y = 0; y < heightMap.GetHeight(); y++)
	    {
		    for (var x = 0; x < heightMap.GetWidth(); x++)
		    {
			    var pixel = new Vector2I(x, y);
			    var value = noisy.GetNoise2Dv(pixel);
			    heightMap.SetPixelv(pixel, new Color(value, value, value, value));
			    max = Mathf.Max(max, value);
			    min = Mathf.Min(min, value);
		    }
	    }
	    
	    for (var y = 0; y < heightMap.GetHeight(); y++)
	    {
		    for (var x = 0; x < heightMap.GetWidth(); x++)
		    {
			    var pixel = new Vector2I(x, y);
			    var normalizedHeight = (heightMap.GetPixelv(pixel).R - min) / (max - min);
			    var islandHeight = 1 - Utils.DistanceEuclideanSquared(pixel, new Rect2(0, 0, UnscaledWidth, UnscaledHeight));
			    // var finalHeight = Mathf.Lerp(noiseHeight, 1 - islandHeight, 1 - islandHeight);
			    var finalHeight= (0.5f * normalizedHeight + 0.5f * islandHeight);
			    heightMap.SetPixelv(pixel, new Color(finalHeight, finalHeight, finalHeight, finalHeight));
		    }
	    }
	    
	    BuildMap(map, heightMap);
    }

    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public Vector2 ToMap(Vector2 globalPosition)
    {
	    return GetProvenceGlobal(globalPosition)?.Position ?? Vector2.Zero;
    }
    
    public Province GetProvenceGlobal(Vector2 globalPosition)
	{
		return Provinces
			.OrderBy(provence => provence.Position.DistanceSquaredTo(globalPosition))
			.ToList()
			.FirstOrDefault(provence => Geometry2D.IsPointInPolygon(globalPosition, provence.Vertices.ToArray()));
	}

	public Province GetProvencePercent(Vector2 percent)
	{
		Debug.Assert(percent.X is >= 0.0f and <= 1.0f && percent.Y is >= 0.0f and <= 1.0f);
		// Since this is Vorinoi we can just find the closest province to the percent.
		return Provinces
			.MinBy(provence => percent.DistanceSquaredTo(provence.PositionPercent));
	}
	
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	private void BuildMap(VoronoiMap voronoiMap, Image heightMap)
	{
		Dictionary<VoronoiCell, Biome> biomes = new();
    	
		foreach (var cell in voronoiMap.Cells.Values)
		{
			var elevation = (heightMap.GetPixelv(cell.Position.ToVector2I()).R - 0.5f) * 2.0f;
			biomes[cell] = elevation < 0.0f ? Biome.Ocean : Biome.Grassland;
		}

		voronoiMap.Cells.Values.ToList().ForEach(cell =>
		{
			Provinces.Add(new Province
			{
				Position = cell.Position * _scale,
				PositionPercent = cell.Position / new Vector2(UnscaledWidth, UnscaledHeight),
				Vertices = cell.Vertices.Select(vert => vert * _scale).ToList(),
				Biome = biomes[cell],
				Resource = GetResource(),
			});
		});
	}
	
	private string GetResource()
	{
		List<string> resources = ["Coal Ore", "Stone", "Copper Ore", "Tin Ore", "Mana", "Bone Fragments"];
		return resources.RandomElement();
	}
}