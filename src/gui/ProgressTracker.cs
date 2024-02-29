using Godot;

public partial class ProgressTracker : ProgressBar
{
	[Export] public Node NodeToTrack;

	public ProgressTracker()
	{
		ShowPercentage = false;
	}
	
	public override void _Process(double delta)
	{
		if (NodeToTrack is Building building && building.RemovePercent > 0.0f)
		{
			Visible = true;
			Value = building.RemovePercent * 100;
			Modulate = new Color(1, 1, 0);
		}
		else if (NodeToTrack is IProgress progress && progress.GetProgressPercent() > 0.0f)
		{
			Modulate = new Color(1, 1, 1);
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
