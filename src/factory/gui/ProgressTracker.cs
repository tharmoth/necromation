using Godot;

public partial class ProgressTracker : ProgressBar
{
	private IProgress _nodeToTrack;

	public void Init(IProgress nodeToTrack)
	{
		_nodeToTrack = nodeToTrack;
		ShowPercentage = false;
	}

	public override void _Process(double delta)
	{
		if (_nodeToTrack == null) return;
		if (_nodeToTrack.GetProgressPercent() > 0.0f)
		{
			Visible = true;
			Value = _nodeToTrack.GetProgressPercent() * 100;
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
