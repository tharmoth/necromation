using Godot;
using System;

public partial class TechnologyButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();
		var press = new InputEventAction();
		press.Action = "open_technology";
		press.Pressed = true;
		Input.ParseInputEvent(press);
		var release = new InputEventAction();
		release.Action = "open_technology";
		release.Pressed = false;
		Input.ParseInputEvent(release);
	}
}
