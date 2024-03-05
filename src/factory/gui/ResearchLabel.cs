using Godot;
using System;
using Necromation;

public partial class ResearchLabel : Label
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = Globals.CurrentTechnology?.Name ?? "No Research Selected";
	}
}
