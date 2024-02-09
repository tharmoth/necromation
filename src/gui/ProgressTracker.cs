using Godot;
using System;

public partial class ProgressTracker : ProgressBar
{
	[Export] public Node NodeToTrack;

	public override void _Process(double delta)
	{
		if (NodeToTrack is IProgress progress && progress.GetProgressPercent() > 0.0f)
		{
			Visible = true;
			Value = progress.GetProgressPercent() * 100;
		}
		else
		{
			Visible = false;
			Value = 0;
		}
	}
	
	public interface IProgress
	{
		/*
		 * Returns progress of the tracked node from 0.0 to 1.0
		 */
		public float GetProgressPercent();
	}
}
