using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class VoronoiCell
{
	public required Vector2 Position { get; init; }
	public readonly List<Vector2> Vertices = [];
	public readonly List<VoronoiCell> Neighbors = [];
}

public class VoronoiMap
{
	private readonly int _width;
	private readonly int _height;
	public readonly Dictionary<Vector2, VoronoiCell> Cells = [];

	public VoronoiMap(int cellCount, int width, int height)
	{
		_width = width;
		_height = height;
		var points = GetBlueNoise(cellCount, width, height);
		
		VoronoiCalculate(points);

		for (var i = 0; i < 2; i++)
		{
			LLoydRelaxation();
		}
	}
	
	private static List<Vector2> GetBlueNoise(int pointCount, int maxX, int maxY)
	{
		// Seed the list with a starting point
		List<Vector2> points = [new Vector2(GD.Randf() * maxX, GD.Randf() * maxY)];
		for (var i = 0; i < pointCount; i++)
		{
			// Generate 10 random points and choose the one furthest from existing points
			var closestCandidate = 
				Enumerable.Range(0, 10)
					.Select(_ => new Vector2(GD.Randf() * maxX, GD.Randf() * maxY))
					.MaxBy(candidate => points.Select(candidate.DistanceSquaredTo).Min());
			points.Add(closestCandidate);
		}
		return points;
	}
	
    private void VoronoiCalculate(List<Vector2> points)
	{
		Cells.Clear();
		points.ForEach(point => Cells.Add(point, new VoronoiCell { Position = point }));

		// Calculate the Delaunay triangulation
		var triangles = Geometry2D.TriangulateDelaunay(points.ToArray()).ToList();

		// Calculate the vertices of the Vornoi cells via the circumcenters of the Delaunay triangles.
		for (var i = 0; i < triangles.Count; i += 3)
		{
			var pointOneIndex   = triangles[i];
			var pointTwoIndex   = triangles[i + 1];
			var pointThreeIndex = triangles[i + 2];
			var pointOne = points[pointOneIndex];
			var pointTwo = points[pointTwoIndex];
			var pointThree = points[pointThreeIndex];
				
			var cumCenter = GetCircumcenter(pointOne, pointTwo, pointThree);
			if (cumCenter.X > _width || cumCenter.X < 0 || cumCenter.Y > _height || cumCenter.Y < 0) continue;

			Cells[pointOne].Vertices.Add(cumCenter);
			Cells[pointTwo].Vertices.Add(cumCenter);
			Cells[pointThree].Vertices.Add(cumCenter);
			
			Cells[pointOne].Neighbors.Add(Cells[pointTwo]);
			Cells[pointOne].Neighbors.Add(Cells[pointThree]);
			Cells[pointTwo].Neighbors.Add(Cells[pointOne]);
			Cells[pointTwo].Neighbors.Add(Cells[pointThree]);
			Cells[pointThree].Neighbors.Add(Cells[pointOne]);
			Cells[pointThree].Neighbors.Add(Cells[pointTwo]);
		}

		// The Delaunay triangulation does not include the edges of the map, so we need to add them manually
		FixEdges();
		
		// Order the vertices
		foreach (var cell in Cells.Values)
		{
			var sortedVertices = Geometry2D.ConvexHull(cell.Vertices.ToArray()).ToList();
			cell.Vertices.Clear();
			cell.Vertices.AddRange(sortedVertices);
		}
		
		// Verify that the voronoi centers are inside the cell
		foreach (var cell in Cells.Values)
		{
			Debug.Assert(Geometry2D.IsPointInPolygon(cell.Position, cell.Vertices.ToArray()));
		}
	}
	
	private void LLoydRelaxation()
	{
		var newPoints = Cells.Values
			.Select(cell => cell.Vertices)
			.Select(CalculateCentroid)
			.ToList();
		VoronoiCalculate(newPoints);
	}
	
	private void FixEdges()
	{
		var points = Cells.Keys;
		var currentClosest = points.MinBy(point => point.DistanceSquaredTo(new Vector2(0, 0)));
		Cells[currentClosest].Vertices.Add(new Vector2(0, 0));
		for (var x = 0; x < _width; x++)
		{
			var point = new Vector2(x, 0);
			var closest = points.MinBy(point.DistanceSquaredTo);
			if (closest == currentClosest) continue;
			Cells[closest].Vertices.Add(point);
			Cells[currentClosest].Vertices.Add(point);
			currentClosest = closest;
		}
		Cells[currentClosest].Vertices.Add(new Vector2(_width, 0));
		for (var y = 0; y < _height; y++)
		{
			var point = new Vector2(_width, y);
			var closest = points.MinBy(point.DistanceSquaredTo);
			if (closest == currentClosest) continue;
			Cells[closest].Vertices.Add(point);
			Cells[currentClosest].Vertices.Add(point);
			currentClosest = closest;
		}
		Cells[currentClosest].Vertices.Add(new Vector2(_width, _height));
		for (var x = _width; x >= 0; x--)
		{
			var point = new Vector2(x, _height);
			var closest = points.MinBy(point.DistanceSquaredTo);
			if (closest == currentClosest) continue;
			Cells[closest].Vertices.Add(point);
			Cells[currentClosest].Vertices.Add(point);
			currentClosest = closest;
		}
		Cells[currentClosest].Vertices.Add(new Vector2(0, _height));
		for (var y = _height; y >= 0; y--)
		{
			var point = new Vector2(0, y);
			var closest = points.MinBy(point.DistanceSquaredTo);
			if (closest == currentClosest) continue;
			Cells[closest].Vertices.Add(point);
			Cells[currentClosest].Vertices.Add(point);
			currentClosest = closest;
		}
	}
	
	// Function to get the circumcenter of a triangle
	private static Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
	{
		// Calculate the midpoints of the sides
		Vector2 midAB = (a + b) / 2;
		Vector2 midBC = (b + c) / 2;

		// Calculate the slopes of the sides
		float slopeAB = (b.Y - a.Y) / (b.X - a.X);
		float slopeBC = (c.Y - b.Y) / (c.X - b.X);

		// Calculate the slopes of the perpendicular bisectors
		float perpSlopeAB = -1 / slopeAB;
		float perpSlopeBC = -1 / slopeBC;

		// Calculate the intercepts of the perpendicular bisectors
		float interceptAB = midAB.Y - perpSlopeAB * midAB.X;
		float interceptBC = midBC.Y - perpSlopeBC * midBC.X;

		// Calculate the circumcenter (intersection of the perpendicular bisectors)
		float circumcenterX = (interceptBC - interceptAB) / (perpSlopeAB - perpSlopeBC);
		float circumcenterY = perpSlopeAB * circumcenterX + interceptAB;

		return new Vector2(circumcenterX, circumcenterY);
	}
	
	private static Vector2 CalculateCentroid(List<Vector2> vertices)
	{
		float centroidX = 0;
		float centroidY = 0;
		float signedArea = 0;
		float x0 = 0; // Current vertex X
		float y0 = 0; // Current vertex Y
		float x1 = 0; // Next vertex X
		float y1 = 0; // Next vertex Y
		float a = 0;  // Partial signed area

		int vertexCount = vertices.Count;

		for (int i = 0; i < vertexCount; ++i)
		{
			x0 = vertices[i].X;
			y0 = vertices[i].Y;
			x1 = vertices[(i + 1) % vertexCount].X;
			y1 = vertices[(i + 1) % vertexCount].Y;
			a = x0 * y1 - x1 * y0;
			signedArea += a;
			centroidX += (x0 + x1) * a;
			centroidY += (y0 + y1) * a;
		}

		signedArea *= 0.5f;
		centroidX /= (6 * signedArea);
		centroidY /= (6 * signedArea);

		return new Vector2(centroidX, centroidY);
	}
}