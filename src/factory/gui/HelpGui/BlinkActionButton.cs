using Godot;
using System;

public partial class BlinkActionButton : Button
{
	[Export] private string _action = "open_technology";
	private Tween _tween;
	private bool _shouldBlink = true;
	
	public BlinkActionButton()
	{
		AudioStreamPlayer buttonClick = 
			GD.Load<PackedScene>("res://src/factory/gui/InventoryGui/MouseoverAudio.tscn")
				.Instantiate<AudioStreamPlayer>();
		AddChild(buttonClick);
		Blink();
	}
	
	public override void _Pressed()
	{
		base._Pressed();
		var press = new InputEventAction();
		press.Action = _action;
		press.Pressed = true;
		Input.ParseInputEvent(press);
		_shouldBlink = false;
		_tween?.Kill();
		Modulate = Colors.White;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Blink();
	}

	private void Blink()
	{
		if ((_tween != null && _tween.IsRunning()) || !_shouldBlink) return;
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate", Colors.Yellow, 0.5f);
		_tween.TweenProperty(this, "modulate", Colors.White, 0.5f);
		_tween.TweenCallback(Callable.From(Blink));
	}
}
