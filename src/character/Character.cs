using Godot;
using System;
using Necromation.character;

public partial class Character : Node2D
{
	private float _speed = 100;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("player");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Move the character at speed
		if (Input.IsActionPressed("right"))
		{
			Position += new Vector2(_speed * (float)delta, 0);
		}

		if (Input.IsActionPressed("left"))
		{
			Position += new Vector2(-_speed * (float)delta, 0);
		}
		
		if (Input.IsActionPressed("up"))
		{
			Position += new Vector2(0, -_speed * (float)delta);
		}

		if (Input.IsActionPressed("down"))
		{
			Position += new Vector2(0, _speed * (float)delta);
		}
	}
}
