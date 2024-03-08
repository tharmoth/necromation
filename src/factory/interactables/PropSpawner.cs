using Godot;
using System;
using Godot.Collections;
using Necromation;
using Array = Godot.Collections.Array;

public partial class PropSpawner : Node2D
{
	public enum RandomType
	{
		Random,
		Simplex,
		Cuboid,
		Particles,
	}

	[Export] private RandomType _type = RandomType.Simplex;
	[Export] private Array<Texture2D> _spriteTextures = new();
	[Export] private int _radius = 1000;
	[Export] private float _scale = 0.25f;
	[Export] private int _count = 100;

	private double _time;

	public PropSpawner()
	{
		
	}
	
	public PropSpawner(RandomType type, Array<Texture2D> textures, int radius,float scale, int count = 100)
	{
		_type = type;
		_spriteTextures = textures;
		_radius = radius;
		_scale = scale;
		_count = count;
	}

	public override void _Ready()
	{
		base._Ready();
		// GetNode<Sprite2D>("Sprite2D").Visible = false;

		CallDeferred("_spawn");
	}

	private void _spawn()
	{
		switch (_type)
		{
			case RandomType.Random:
				PlacePropsRandom();
				break;
			case RandomType.Simplex:
				PlacePropsSimplex();
				break;
			case RandomType.Cuboid:
				PlacePropsCuboid();
				break;
			case RandomType.Particles:
				PlacePropsParticles();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void PlacePropsSimplex()
	{
		var _noise = new FastNoiseLite();
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;

		for (var i = 0; i < _count; i++)
		{
			var x = _noise.GetNoise2D(i, 0) * _radius;
			var y = _noise.GetNoise2D(0, i) * _radius;
			PlaceProp(new Vector2(x, y), _scale);
		}
	}

	private void PlacePropsRandom()
	{
		for (var i = 0; i < _count; i++)
		{
			var x = new Random().Next(-_radius, _radius);
			var y = new Random().Next(-_radius, _radius);
			PlaceProp(new Vector2(x, y), _scale);
		}
	}
	
	private void PlacePropsCuboid()
	{
		var count = 0;
		for (var x = -_radius; x < _radius; x += 32)
		{
			for (var y = -_radius; y < _radius; y += 32)
			{
				if (GD.Randf() > .5)
				{
					count += FillCell(new Vector2(x, y));
				}
			}
		}
		GD.Print("Placed " + count + " props");
	}

	private int FillCell(Vector2 globalPosition)
	{
		var count = 0;
		for (var x = 5; x < 32; x += 10)
		{
			for (var y = 5; y < 32; y += 10)
			{
				var scale = _scale * (float)GD.RandRange(.5f, 1f);
				if (GD.Randf() > .75)
				{
					PlaceProp(globalPosition + new Vector2(x + GD.RandRange(-3, 3), y + GD.RandRange(-3, 3)), scale);
					count++;
				}
			}
		}

		return count;
	}
	
	private void PlaceProp(Vector2 position, float scale)
	{
		var spawn = new Sprite2D();
		spawn.Texture = _spriteTextures[new Random().Next(0, _spriteTextures.Count - 1)];
		spawn.Position = position;
		spawn.ZIndex = -98;
		spawn.RotationDegrees = new Random().Next(-10, 10);
		spawn.Scale = new Vector2(scale, scale);
		// ShaderMaterial matty = new();
		// matty.Shader = GD.Load<Shader>("res://src/factory/shaders/wind_sway.gdshader");
		// matty.SetShaderParameter("offset", GlobalPosition.X + position.X + GlobalPosition.Y + position.Y);
		// matty.SetShaderParameter("minStrength", 0.025f);
		// matty.SetShaderParameter("maxStrength", 0.1f);
		// matty.SetShaderParameter("detail", 5.0f);
		// spawn.Material = matty;
		// AddChild(spawn);
	}

	private GpuParticles2D party;
	
	public void PlacePropsParticles()
	{
		party = GD.Load<PackedScene>("res://src/factory/shaders/grass_particles.tscn").Instantiate<GpuParticles2D>();
		AddChild(party);
		party.ZIndex = -98;
		var tweeny = Globals.Tree.CreateTween();
		tweeny.TweenInterval(1);
		tweeny.TweenCallback(Callable.From(() => party.SpeedScale = 0));
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (party == null) return;

		var matty = party.Material as ShaderMaterial;
		matty.SetShaderParameter("character_position", Globals.Player.GlobalPosition);
	}
}
