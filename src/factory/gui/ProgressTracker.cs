using Godot;

public partial class ProgressTracker : ProgressBar
{
	public IProgress NodeToTrack;

	private ProgressTracker()
	{
		
	}

	public ProgressTracker(IProgress progress)
	{
		ShowPercentage = false;
	}

	public override void _Ready()
	{
		base._Ready();
		if (GetParent() is IProgress progress)
		{
			NodeToTrack = progress;
		}
		else
		{
			GD.PrintErr("ProgressTracker: Parent does not implement IProgress" + GetParent().GetPath());
		}
	}

	// public override void _Process(double delta)
	// {
	// 	if (NodeToTrack == null) return;
	// 	var node = (Node2D)NodeToTrack;
	// 	if (NodeToTrack.GetProgressPercent() > 0.0f)
	// 	{
	// 		if (!node.Visible) node.Visible = true;
	// 		Value = NodeToTrack.GetProgressPercent() * 100;
	// 	}
	// 	else
	// 	{
	// 		if (node.Visible) node.Visible = false;
	// 		Value = 0;
	// 	}
	// }
	
	public interface IProgress
	{
		/*
		 * Returns progress of the tracked node from 0.0 to 1.0
		 */
		public float GetProgressPercent();
	}
}
