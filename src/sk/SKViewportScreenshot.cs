using Godot;
using System;

public partial class SKViewportScreenshot : Node2D
{
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("left_click"))
		{
			var viewport = GetViewport();
			var screenshot = viewport.GetTexture().GetImage();
			screenshot.SavePng("screenshot.png");
			GD.Print("Screenshot saved as screenshot.png");
		}
	}
}
