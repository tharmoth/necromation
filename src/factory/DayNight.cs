using Godot;
using System;
using Necromation;

public partial class DayNight : CanvasModulate
{
	private static double Time = STARTING_HOUR * MINUTES_PER_HOUR * INGAME_TO_REAL_MINUTE;
	
	const int MINUTES_PER_DAY = 1440;
	private const int MINUTES_PER_HOUR = 60;
	private const double INGAME_TO_REAL_MINUTE = (2 * Mathf.Pi) / MINUTES_PER_DAY;
	private const double MINUTES_PER_SECOND = 1.2;
	private const float STARTING_HOUR = 1.0f;
	
	[Export] private GradientTexture1D _gradient;
	
	public static void SetHour(float hour)
	{
		Time = hour * MINUTES_PER_HOUR * INGAME_TO_REAL_MINUTE;
	}
	
	public static float GetHour()
	{
		return (float)(Time / (MINUTES_PER_HOUR * INGAME_TO_REAL_MINUTE));
	}
	
	public override void _Ready()
	{
		base._Ready();
		if (Time < 0)
		{
			Time = STARTING_HOUR * MINUTES_PER_HOUR * INGAME_TO_REAL_MINUTE;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Time += delta * INGAME_TO_REAL_MINUTE * MINUTES_PER_SECOND;
		
		// Reset time to 0 once a day has passed
		if (GetHour() > 24)
		{
			Time = 0;
		}
		
		var value = (Mathf.Sin(Time) + 1) / 2;

		if (Globals.Player != null)
		{
			Globals.Player.Light.Enabled = GetHour() > 13 && GetHour() < 23;
		}
		
		Color = _gradient.Gradient.Sample((float)value);
	}
}
