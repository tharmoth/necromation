using Godot;
using System;
using Necromation;

public partial class DayNight : CanvasModulate
{
	const int MINUTES_PER_DAY = 1440;
	const int MINUTES_PER_HOUR = 60;
	private const double INGAME_TO_REAL_MINUTE = (2 * Mathf.Pi) / MINUTES_PER_DAY;
	
	
	[Export] private GradientTexture1D _gradient;
	[Export] private double MINUTES_PER_SECOND = 1.2;
	[Export] private float STARTING_HOUR = 0.0f;

	private double _time;

	public override void _Ready()
	{
		base._Ready();
		_time = STARTING_HOUR * MINUTES_PER_HOUR * INGAME_TO_REAL_MINUTE;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_time += delta * INGAME_TO_REAL_MINUTE * MINUTES_PER_SECOND;
		var value = (Mathf.Sin(_time) + 1) / 2;

		if (Globals.Player != null)
		{
			Globals.Player.Light.Enabled = value < .25;
		}
		
		Color = _gradient.Gradient.Sample((float)value);
	}
}
