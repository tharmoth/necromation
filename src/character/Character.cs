using Godot;
using System;
using Necromation;
using Necromation.character;

public partial class Character : Node2D
{
	Inventory _inventory = new Inventory();
	private Object _selected = null;
	private Tween _tween;

	public Object Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			if (value is string selectedString) _sprite.Texture = GD.Load<Texture2D>($"res://res/sprites/{selectedString}.png");
			else
			{
				_sprite.Texture = GD.Load<Texture2D>("res://res/sprites/selection.png");
			}
		}
	}
	private Sprite2D _sprite;
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Globals.PlayerInventory = _inventory;
		Globals.Player = this;
	}

	private float _speed = 100;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	
	
	
	{
		AddToGroup("player");
		_inventory.AddItem("Stone", 500);
		_sprite = new Sprite2D();
		_sprite.ZIndex = 1;
		GetTree().Root.CallDeferred("add_child", _sprite);
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
		
		if (Input.IsMouseButtonPressed(MouseButton.Left) && GetBuildingAtMouse() is Interactable building) building.Interact();

		ProcessCursor();
	}

	private void ProcessCursor()
	{
		if (Selected != null)
		{
			ProcessCursorSelected();
		}
		else if (Globals.TileMap.GetBuilding(GetGlobalMousePosition()) != null)
		{
			ProcessCursorMouseover();
		}
		else
		{
			_sprite.Visible = false;
		}
	}

	private void ProcessCursorMouseover()
	{
		_sprite.Visible = true;
		_sprite.Position = Globals.TileMap.ToMap(GetGlobalMousePosition());
		_sprite.Modulate = Colors.White;

		if (_tween != null) return;

		_tween = GetTree().CreateTween();
		_tween.TweenProperty(_sprite, "modulate", Colors.Transparent, 0.5f);
		_tween.TweenProperty(_sprite, "modulate", Colors.White, 0.5f);
		_tween.TweenCallback(Callable.From(() => _tween = null));
	}

	private void ProcessCursorSelected()
	{
		_tween?.Kill();
		_tween = null;
		_sprite.Visible = true;
		_sprite.Modulate = Globals.TileMap.PositionEmpty(GetGlobalMousePosition()) ? Colors.Green : Colors.Red;
		_sprite.Position = Globals.TileMap.ToMap(GetGlobalMousePosition());
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (Input.IsMouseButtonPressed(MouseButton.Left)) LeftMouseButtonPressed();
		if (Input.IsMouseButtonPressed(MouseButton.Right)) RightMouseButtonPressed();
		
		if (@event is not InputEventMouseButton eventMouseButton) return;
		if (!eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Right) RightClick();
	}
	
	private void LeftMouseButtonPressed()
	{
		if (Selected is string selectedString) Build(selectedString);
	}
	
	private void RightMouseButtonPressed()
	{
		RemoveBuilding();
	}
	
	private void RightClick()
	{
		Selected = null;
	}

	private void Build(string selectedItem)
	{
		if (!_inventory.Items.ContainsKey(selectedItem))
		{
			Selected = null;
			return;
		}
		
		var position = GetGlobalMousePosition();
		if (!Globals.TileMap.PositionEmpty(position)) return;
		
		_inventory.RemoveItem(selectedItem);

		var building = GD.Load<PackedScene>("res://src/interactables/building.tscn").Instantiate<Node2D>();
		Globals.TileMap.Build(position, building as BuildingTileMap.IBuilding);
		
		if(!_inventory.Items.ContainsKey(selectedItem)) Selected = null;
	}
	
	private void RemoveBuilding()
	{
		var building = GetBuildingAtMouse();
		if (building is null) return;
		Globals.TileMap.Remove(building);
		_inventory.AddItem(building.ItemType, 1);
	}

	private BuildingTileMap.IBuilding GetBuildingAtMouse()
	{
		return Globals.TileMap.GetBuilding(GetGlobalMousePosition());
	}
}
