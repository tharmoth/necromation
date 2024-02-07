using Godot;
using System;
using Necromation;

public partial class Spawner : Node2D
{
	[Export] private PackedScene _type;
	[Export] private string _resourceType = "Stone";
	[Export] private int _range = 100;
	[Export] private int _timeSeconds = 5;
	[Export] private int _maxSpawned = 5;
	
	private double _time;

	public override void _Ready()
	{
		base._Ready();
		GetNode<Sprite2D>("Sprite2D").Visible = false;
		_spawn();
		_spawn();
		_spawn();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_time += delta;
		if (_time > _timeSeconds && GetChildCount() <= _maxSpawned)
		{
			_time = 0;
			_spawn();
		}
	}

	private void _spawn()
	{
		var spawn = _type.Instantiate<Collectable>();
		spawn.Type = _resourceType;
		// Place the scene within the spawner's range
		AddChild(spawn);
		spawn.GlobalPosition = GlobalPosition + new Vector2((float)GD.RandRange(-_range, _range), (float)GD.RandRange(-_range, _range));
	}
	
}
