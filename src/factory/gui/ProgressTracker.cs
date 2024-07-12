using System;
using Godot;

public partial class ProgressTracker : ProgressBar
{
	private Func<float> _getProgress;

	public void Init(Func<float> getProgress)
	{
		_getProgress = getProgress;
		ShowPercentage = false;
	}

	public override void _Process(double delta)
	{
		if (_getProgress == null) return;
		var value = _getProgress.Invoke();
		if (value > 0.0f)
		{
			Visible = true;
			Value = value * 100;
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
