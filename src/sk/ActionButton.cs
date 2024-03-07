using Godot;
using System;

public partial class ActionButton : Button
{
	[Export] private string _action = "open_technology";

	public ActionButton()
	{
		AudioStreamPlayer buttonClick = 
			GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/mouseover_audio.tscn")
			.Instantiate<AudioStreamPlayer>();
		AddChild(buttonClick);
	}
	
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
