using Godot;
using System;
using System.Linq;
using Necromation;

public partial class SoulsLabel : Label
{
	private int _souls = 0;
	private Tween _tween;
	private bool _shouldBlink = false;
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = "Souls: " + Globals.Souls;

		if (_souls != Globals.Souls) Blink();
		_shouldBlink = ShouldBlink();
		Blink();
		Globals.SoulListeners.ForEach(listener => listener());
	}

	private bool ShouldBlink()
	{
		return Database.Instance.Technologies
			.Where(tech => !tech.Researched)
			.Any(tech => tech.Count < Globals.Souls);
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
