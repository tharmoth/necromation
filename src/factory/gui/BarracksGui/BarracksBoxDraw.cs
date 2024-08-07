using System.Linq;
using Godot;
using Necromation.map.character;
using Necromation;

public partial class BarracksBoxDraw : Control
{
	/************************************************************************
	 * Hardcoded Scene Imports 												*
	 ************************************************************************/
	private readonly static PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/BarracksGui/BarracksBoxDraw.tscn");
	
	/************************************************************************
	 * Child Accessors 													    *
	 ************************************************************************/
	private Sprite2D BoxSprite => GetNode<Sprite2D>("%Sprite");
	private TextureRect BackgroundTexture => GetNode<TextureRect>("%BackgroundTexture");
	private Container BoxDraw => GetNode<Container>("%BoxDraw");
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private Commander _commander;
	private bool _dragging = false;
	
	/**************************************************************************
	 * Constants         													  *
	 **************************************************************************/
	private readonly Vector2I _gridSize = new(100, 100);
	
	public void Init(Commander commander)
	{
		_commander = commander;
	}
	
	public override void _Ready()
	{
		base._Ready();
		// Place boxsprite at the commanders spawn location

		CallDeferred("AddCommander", _commander, BoxSprite);
		BackgroundTexture.GuiInput += @event =>
		{
			if (!@event.IsActionPressed("left_click")) return;
			_dragging = true;
		};
		BoxSprite.Texture = new Texture2D();
		BoxSprite.ZIndex = 1;
		_commander.Units.Listeners.Add(UpdateSprites);
		UpdateSprites();
		
		// _commander.Province.Commanders.ForEach(commander =>
		// {
		// 	if (commander == _commander) return;
		// 	Sprite2D sprite = new();
		// 	sprite.Scale = Vector2.One * 0.25f;
		// 	sprite.Texture = new Texture2D();
		// 	sprite.Modulate = new Color(1, 1, 1, 0.33f);
		// 	BoxDraw.AddChild(sprite);
		// 	UpdateSprites(commander, sprite);
		// 	CallDeferred("AddCommander", commander, sprite);
		// });
	}

	private void AddCommander(Commander commander, Sprite2D sprite)
	{
		var spawnLocation = commander.SpawnLocation;

		var x = spawnLocation.X / (float)_gridSize.X;
		var y = spawnLocation.Y / (float)_gridSize.Y;
		
		var size = BackgroundTexture.GetRect().Size;

		UpdateSpritePosition(sprite, new Vector2(x, y) * size);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!_dragging) return;
		if (!Input.IsActionPressed("left_click"))
		{
			_dragging = false;
			return;
		}

		// find out where on box the mouse is
		var diff = BackgroundTexture.GetGlobalMousePosition() - BackgroundTexture.GlobalPosition;
		var size = BackgroundTexture.GetRect().Size;
		var x = diff.X / size.X;
		var y = diff.Y / size.Y;
		
		x = Mathf.Clamp(x, 0, 1);
		y = Mathf.Clamp(y, 0, 1);
		
		// if (!(x < 1f) || !(x >= 0) || !(y < 1f) || !(y >= 0)) return;

		var gridX = Mathf.RoundToInt(_gridSize.X * x);
		var gridY = Mathf.RoundToInt(_gridSize.Y * y);

		_commander.SpawnLocation = new Vector2I(gridX, gridY);
		UpdateSpritePosition(BoxSprite, new Vector2(x, y) * size);
	}

	private void UpdateSprites(Commander commander, Node spriteHolder)
	{
		float x = 0;
		float y = 0;
		float offset = 0;
		spriteHolder.GetChildren().ToList().ForEach(node => node.QueueFree());
		foreach (var (unitType, count) in commander.Units.Items)
		{
			for (var i = 0; i < count; i += 10)
			{
				var subSprite = new Sprite2D();
				subSprite.Texture = Database.Instance.GetTexture(unitType);
				// subSprite.Scale = Vector2.One * (MapScene.MapCellSize / 6.0f) / subSprite.Texture.GetWidth();
				subSprite.Position = new Vector2(-y * 10, x + offset) * 15;
				subSprite.YSortEnabled = true;
				spriteHolder.AddChild(subSprite);
                
				x += 1;
				if (!(x >= 10)) continue;
				x = 0;
				offset = offset > 0 ? 0 : .5f;
				y += 1;
			}
		}
		
		if (commander.Units.Items.Count == 0)
		{
			var subSprite = new Sprite2D();
			subSprite.Texture = Database.Instance.GetTexture("Skeleton Auxiliary");
			subSprite.Position = new Vector2(-y * 10, x + offset) * 15;
			subSprite.YSortEnabled = true;
			spriteHolder.AddChild(subSprite);
		}
	}

	private void UpdateSpritePosition(Node2D spriteHolder, Vector2 localPos)
	{
		spriteHolder.GlobalPosition = localPos + BackgroundTexture.GlobalPosition;
		var size = BackgroundTexture.GetRect().Size;
		foreach (var subSprite in spriteHolder.GetChildren().OfType<Sprite2D>())
		{
			localPos = spriteHolder.GlobalPosition - BackgroundTexture.GlobalPosition;
			var spriteSize = subSprite.Texture.GetSize() * spriteHolder.Scale;
			var spritePos = subSprite.GlobalPosition - spriteHolder.GlobalPosition;
			// Right
			if (localPos.X + spriteSize.X / 2 + spritePos.X > size.X)
			{
				localPos = new Vector2(size.X - spriteSize.X / 2, localPos.Y);
			}

			// Left
			if (localPos.X - spriteSize.X / 2 + spritePos.X < 0)
			{
				localPos = new Vector2(spriteSize.X / 2 - spritePos.X , localPos.Y);
			}
			
			// Down
			if (localPos.Y + spriteSize.Y / 2 + spritePos.Y > size.Y)
			{
				localPos = new Vector2(localPos.X, size.Y - spriteSize.Y / 2 - spritePos.Y);
			}
		
			// Up
			if (localPos.Y - spriteSize.Y / 2 + spritePos.Y < 0)
			{
				localPos = new Vector2(localPos.X, spriteSize.Y / 2);
			}
			spriteHolder.GlobalPosition = localPos + BackgroundTexture.GlobalPosition;
		}
	}

	private void UpdateSprites()
	{
		UpdateSprites(_commander, BoxSprite);
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		_commander.Units.Listeners.Remove(UpdateSprites);
	}
}