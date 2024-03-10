using Godot;
using System;

public partial class CloudTest : Sprite2D
{
	float _speed = 100;
	public Vector2 Direction = new Vector2(1, 1).Normalized();

	public CloudTest()
	{
		_speed = 25;
		Texture = Database.Instance.GetTexture("CloudTest");
		ZIndex = 100;
		RotationDegrees = new Random().Next(-180, 180);
		Modulate = new Color(1, 1, 1, .1f + (float)GD.RandRange(-.1f, .1f));
		var scale = (float)GD.RandRange(4, 5);
		Scale = new Vector2(scale, scale);
		Centered = false;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition += Direction * (float)delta * _speed;
		if (!(GlobalPosition.X + GlobalPosition.Y > 6400 + (Texture.GetSize().X + Texture.GetSize().Y) * Scale.X)) return;
		GetParent<CloudRegionSpawner>().Spawn(0);
		QueueFree();
	}
}
