using Godot;
using System;

public partial class PropSpawner : Node2D
{
	private enum RandomType
	{
		Random,
		Simplex,
	}

	[Export] private RandomType type = RandomType.Simplex;
	[Export] private Texture2D _spriteTexture; 
	[Export] private int _radius = 1000;
	[Export] private int count = 100;
	[Export] private float _opacity = 0.25f;

	private double _time;

	public override void _Ready()
	{
		base._Ready();
		GetNode<Sprite2D>("Sprite2D").Visible = false;

		CallDeferred("_spawn");
	}

	private void _spawn()
	{
		switch (type)
		{
			case RandomType.Random:
				PlacePropsRandom();
				break;
			case RandomType.Simplex:
				PlacePropsSimplex();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void PlacePropsSimplex()
	{
		var _noise = new FastNoiseLite();
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;

		for (var i = 0; i < count; i++)
		{
			var x = _noise.GetNoise2D(i, 0) * _radius;
			var y = _noise.GetNoise2D(0, i) * _radius;
			PlaceProp(new Vector2(x, y));
		}
	}

	private void PlacePropsRandom()
	{
		for (var i = 0; i < count; i++)
		{
			var x = new Random().Next(-_radius, _radius);
			var y = new Random().Next(-_radius, _radius);
			PlaceProp(new Vector2(x, y));
		}
	}
	
	private void PlaceProp(Vector2 position)
	{
		var spawn = new Sprite2D();
		spawn.Texture = _spriteTexture;
		spawn.Position = position;
		spawn.ZIndex = -1;
		spawn.Modulate = new Color(1, 1, 1, _opacity);
		spawn.RotationDegrees = new Random().Next(-10, 10);
		AddChild(spawn);
	}
}
