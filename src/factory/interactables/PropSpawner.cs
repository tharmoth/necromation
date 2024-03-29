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
	private Vector2 _globalPosition;
	private int _noiseSeed;
	private bool _clumping;

	private double _time;

	public PropSpawner()
	{
		
	}
	
	public PropSpawner(RandomType type, Array<Texture2D> textures, int radius,float scale, int count = 100, Vector2 globalPosition = default, int noiseSeed = 0, bool clumping = false)
	{
		_type = type;
		_spriteTextures = textures;
		_radius = radius;
		_scale = scale;
		_count = count;
		_globalPosition = globalPosition;
		_noiseSeed = noiseSeed;
		_clumping = clumping;
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
		var _noise = new FastNoiseLite();
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		_noise.Seed = _noiseSeed;
		_noise.Frequency = 0.005f;
		_noise.Offset = new Vector3(_globalPosition.X, _globalPosition.Y, 0);
		
		var count = 0;
		for (var x = -_radius; x < _radius; x += 32)
		{
			for (var y = -_radius; y < _radius; y += 32)
			{
				if (_noise.GetNoise2D(x, y) > 0.5f)
				{
					count += FillCell(new Vector2(x, y));
				}
			}
		}
		GD.Print("Placed " + count + " props");
	}

	private int FillCell(Vector2 position)
	{
		var count = 0;
		
		for (var x = 5; x < 32; x += 10)
		{
			for (var y = 5; y < 32; y += 10)
			{
				var scale = _scale * (float)GD.RandRange(.5f, 1f);
				if (GD.Randf() > .75)
				{
					PlaceProp(position + new Vector2(x + GD.RandRange(-3, 3), y + GD.RandRange(-3, 3)), scale);
					count++;
				}
			}
		}

		return count;
	}
	
	private void PlaceProp(Vector2 position, float scale)
	{
		// if(true) return;
		var spawn = new Sprite2D();
		spawn.Texture = _spriteTextures[new Random().Next(0, _spriteTextures.Count - 1)];
		spawn.Position = position;
		// spawn.ZIndex = -98;
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
		
		var texture = _spriteTextures[new Random().Next(0, _spriteTextures.Count - 1)];
		
		var width = texture.GetWidth() * scale;
		var height = texture.GetHeight() * scale;
		var size = new Vector2(width, height);
		
		Rid _renderingServerId = RenderingServer.CanvasItemCreate();
		RenderingServer.CanvasItemSetParent(_renderingServerId, GetCanvasItem());
		RenderingServer.CanvasItemAddTextureRect(_renderingServerId, new Rect2(position - size / 2, size), texture.GetRid());
		var transform = Transform2D.Identity.Translated(position);
		RenderingServer.CanvasItemSetTransform(_renderingServerId, transform);
		// RenderingServer.CanvasItemSetZIndex(_renderingServerId, -98);
		
	}

	private GpuParticles2D party;
	
	public void PlacePropsParticles()
	{
		party = GD.Load<PackedScene>("res://src/factory/shaders/grass_particles.tscn").Instantiate<GpuParticles2D>();
		AddChild(party);
		party.Scale = new Vector2(_radius / 320.0f, _radius / 320.0f);
		var tweeny = Globals.Tree.CreateTween();
		tweeny.TweenInterval(1);
		tweeny.TweenCallback(Callable.From(() => party.SpeedScale = 0));
	}

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// 	if (party == null) return;
	//
	// 	var matty = party.Material as ShaderMaterial;
	// 	matty.SetShaderParameter("character_position", Globals.Player.GlobalPosition);
	// }
}
