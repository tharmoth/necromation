using Godot;
using System;

public partial class SwordTest : Node2D
{
	private Sprite2D Sword => GetNode<Sprite2D>("%Sword");
	private Node2D Pivot => GetNode<Node2D>("%Pivot");
	private AudioStreamPlayer Audio => GetNode<AudioStreamPlayer>("%Audio");
	private Sprite2D Swing => GetNode<Sprite2D>("%Swing");
	private CpuParticles2D Particles => GetNode<CpuParticles2D>("%Particles");
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		var direction = new Vector2(
			Input.GetAxis("left", "right"), Input.GetAxis("up", "down")
		);
		
		Pivot.GlobalPosition += direction * 1000 * (float)delta;
		
		Pivot.LookAt(GetGlobalMousePosition());
		// Pivot.GlobalPosition = GetGlobalMousePosition();
		
		if (Input.IsActionJustPressed("left_click"))
		{
			Swing.Visible = true;
			Swing.Modulate = new Color(1, 1, 1, 0);
			var tween = CreateTween();
			Sword.RotationDegrees = 0;
			tween.TweenProperty(Sword, "rotation_degrees", -110,  0.025f);
			tween.TweenProperty(Sword, "rotation_degrees", 110,  0.25f);
			tween.TweenProperty(Sword, "rotation_degrees", 0,  0.05f);
			
			Audio.Play();
			
			var tween2 = CreateTween();
			tween2.TweenInterval(.075 + .025);
			tween2.TweenCallback(Callable.From(() => Particles.Emitting = true));
			
			var tween3 = CreateTween();
			tween3.TweenInterval(0.025);
			tween3.TweenCallback(Callable.From(() => Swing.Visible = true));
			tween3.TweenProperty(Swing, "modulate", new Color(1, 1, 1, 1), .25);
			tween3.TweenCallback(Callable.From(() => Swing.Visible = false));
		}
	}
}
