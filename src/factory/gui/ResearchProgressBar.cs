using Godot;
using System;
using Necromation;

public partial class ResearchProgressBar : ProgressBar
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var research = Globals.CurrentTechnology;
		if (research == null)
		{
			Value = 0;
			return;
		}
		Value = research.Progress / (double) research.Count * 100.0f;
	}
}
