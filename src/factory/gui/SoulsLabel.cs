using Godot;
using System;
using Necromation;

public partial class SoulsLabel : Label
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = "Souls: " + Globals.Souls;
	}
}
