using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.sk;

public partial class ProvinceBorders : Node2D
{
	private readonly List<Line2D> _lines = new();
	
	private class Line
	{
		public Vector2 Start;
		public Vector2 End;
		public Line(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}
	}

	/// <summary>
	/// I think this can be expanded for arbitrary polygons in the future. See:
	/// <a href="https://stackoverflow.com/questions/3501383/find-the-outline-of-a-union-of-grid-aligned-squares">
	/// StackOverflow</a>.
	/// </summary>
	public void UpdateBorders()
	{
		var playerProv = Globals.MapScene.TileMap.Provinces.Where(province => province.Owner == "Player").ToList();

		var lines = playerProv
			.Select(province => province.GetCorners(Globals.MapScene.TileMap.MapToGlobal(province.MapPosition)))
			.ToList()
			.SelectMany(corners => corners.Select((t, i) => new Line(t, corners[(i + 1) % corners.Count])))
			.ToList();
		
		// prune any lines that go in opposite directions.
		List<Line> linesToPrune = new();
		lines.ForEach(line => lines
			.Where(line2 => line2 != line && Utils.IsEqualApprox(line2.Start, line.End) &&
			                Utils.IsEqualApprox(line2.End, line.Start)).ToList().ForEach(line2 =>
		{
			linesToPrune.Add(line);
			linesToPrune.Add(line2);
		}));
		linesToPrune.ForEach(line => lines.Remove(line));
		
		_lines.ForEach(line => line.Free());
		_lines.Clear();
		lines.ForEach(line =>
		{
			var line2D = new Line2D();
			line2D.Points = new[] { line.Start, line.End };
			line2D.Width = 2;
			line2D.EndCapMode = Line2D.LineCapMode.Box;
			line2D.BeginCapMode = Line2D.LineCapMode.Box;
			line2D.DefaultColor = Globals.PlayerColor;
			_lines.Add(line2D);
			AddChild(line2D);
			// CallDeferred("add_child", line2D);
		});
	}
}
