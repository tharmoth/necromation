using Godot;
using System;

public partial class ActionButton : Button
{
	[Export] private string _action = "open_technology";
	
	public override void _Pressed()
	{
		base._Pressed();
		var press = new InputEventAction();
		press.Action = _action;
		press.Pressed = true;
		Input.ParseInputEvent(press);
		var release = new InputEventAction();
		release.Action = _action;
		release.Pressed = false;
		Input.ParseInputEvent(release);
	}
}
