using Godot;
using System;

public partial class FpsCounter : Label
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = Engine.GetFramesPerSecond().ToString();
	}
}
