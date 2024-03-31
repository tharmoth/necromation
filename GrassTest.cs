using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GrassTest : Node2D
{
	[Export] private int _spawnCount = 100000;
	private OptionButton OptionButton => GetNode<OptionButton>("%OptionButton");
	private GpuParticles2D GpuParticles2D => GetNode<GpuParticles2D>("%GPUParticles2D");
	private SpinBox SpinBox => GetNode<SpinBox>("%SpinBox");
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SpinBox.Value = _spawnCount;
		SpinBox.ValueChanged += value =>
		{
			GD.Print("SpinBox.ValueChanged: ", value);
			_spawnCount = (int)value;
			GpuParticles2D.Amount = _spawnCount;
		};
		OptionButton.ItemSelected += OnOptionButtonItemSelected;
		GpuParticles2D.Amount = _spawnCount;
	}

	private bool _paused = false;
	private double _time = 0;
	private int _counter = 0;
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!GpuParticles2D.Visible) return;
		_counter++;
		if (_counter < 2) return;
		if (!_paused) Pause();
	}

	private void Pause()
	{
		_paused = true;
		GpuParticles2D.ProcessMode = ProcessModeEnum.Disabled;
	}
	
	private void OnOptionButtonItemSelected(long index)
	{
		Clear();
		GD.Print(OptionButton.Text);
		switch (OptionButton.Text)
		{
			case "Sprite":
				SpawnGrassSprite();
				break;
			case "Render Server":
				SpawnGrassRenderServer();
				break;
			case "Mesh":
				SpawnGrassMesh();
				break;
			case "Multimesh":
				SpawnGrassMultiMesh();
				break;
			case "Particle":
				GpuParticles2D.Visible = true;
				break;
			case "None":
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	private void Clear()
	{
		GetChildren().OfType<Sprite2D>().ToList().ForEach(node => node.QueueFree());
		_renderingServerIds.ForEach(RenderingServer.CanvasItemClear);
		_renderingServerIds.ForEach(RenderingServer.FreeRid);
		GetChildren().OfType<MeshInstance2D>().ToList().ForEach(node => node.QueueFree());
		GetChildren().OfType<MultiMeshInstance2D>().ToList().ForEach(node => node.QueueFree());
		GpuParticles2D.Visible = false;
		_counter = 0;
	}
	
	private void SpawnGrassSprite()
	{
		for (var i = 0; i < _spawnCount; i++)
		{
			var sprite = new Sprite2D();
			sprite.Texture = Database.Instance.GetTexture("Grass2");
			sprite.GlobalPosition = GetRandomPosition();
			AddChild(sprite);
		}
	}
	
	private List<Rid> _renderingServerIds = new();
	
	private void SpawnGrassRenderServer()
	{
		for (var i = 0; i < _spawnCount; i++)
		{
			var renderingServerId = RenderingServer.CanvasItemCreate();
			_renderingServerIds.Add(renderingServerId);
			RenderingServer.CanvasItemSetParent(renderingServerId, GetCanvasItem());
			
			var texture = Database.Instance.GetTexture("Grass2");
			RenderingServer.CanvasItemAddTextureRect(renderingServerId, new Rect2(GlobalPosition - texture.GetSize() / 2, texture.GetSize()), texture.GetRid());
			
			var transform = Transform2D.Identity.Translated(GetRandomPosition());
			RenderingServer.CanvasItemSetTransform(renderingServerId, transform);
		}
	}
	
	private void SpawnGrassMesh()
	{
		for (var i = 0; i < _spawnCount; i++)
		{
			MeshInstance2D mesh = new();
			mesh.Mesh = new QuadMesh();
			mesh.Texture = Database.Instance.GetTexture("Grass2");
			mesh.Position = GetRandomPosition();
			mesh.RotationDegrees = 180;
			mesh.Scale = mesh.Texture.GetSize();
			AddChild(mesh);
		}
	}
	
	MultiMesh multiMesh = new();
	private int meshCount = 0;
	private void SpawnGrassMultiMesh()
	{
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;
		multiMesh.Mesh = new QuadMesh();
		multiMesh.InstanceCount = _spawnCount;
		
		MultiMeshInstance2D instance = new();
		instance.Multimesh = multiMesh;
		instance.Texture = Database.Instance.GetTexture("Grass2");
		AddChild(instance);
		
		for (var i = 0; i < _spawnCount; i++)
		{
			var transform = Transform2D.Identity
				.Translated(GetRandomPosition())
				.RotatedLocal(Mathf.Pi)
				.ScaledLocal(instance.Texture.GetSize());
			multiMesh.SetInstanceTransform2D(meshCount, transform);
			meshCount++;
		}
	}
	
	private Vector2 GetRandomPosition()
	{
		return new Vector2((float)GD.RandRange(0, GetViewportRect().Size.X), (float)GD.RandRange(0, GetViewportRect().Size.Y));
	}
}
