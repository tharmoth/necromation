using Godot;
using System;
using Necromation;
using Necromation.character;

public partial class Character : Node2D
{
	Inventory _inventory = new Inventory();
	private Object _selected = null;

	public Object Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			if (value is string selectedString) _sprite.Texture = GD.Load<Texture2D>($"res://res/sprites/{selectedString}.png");
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

		if (Selected != null)
		{
			_sprite.Visible = true;
			var position = GetGlobalMousePosition();
			if (Globals.TileMap.CanBuild(position))
			{
				_sprite.Modulate = new Color(0, 1, 0);
			}
			else
			{
				_sprite.Modulate = new Color(1, 0, 0);
			}
			_sprite.Position = Globals.TileMap.ToMap(position);
		}
		else
		{
			_sprite.Visible = false;
		}
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
		if (Selected is null) return;
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
		if (!Globals.TileMap.CanBuild(position)) return;
		
		_inventory.RemoveItem(selectedItem);

		var building = GD.Load<PackedScene>("res://src/interactables/building.tscn").Instantiate<Node2D>();
		Globals.TileMap.Build(position, building as BuildingTileMap.IBuilding);
	}
	
	private void RemoveBuilding()
	{
		var position = GetGlobalMousePosition();
		var building = Globals.TileMap.GetBuilding(position);
		if (building is null) return;
		Globals.TileMap.Remove(Globals.TileMap.GlobalToMap(position));
		_inventory.AddItem(building.ItemType, 1);
	}
}
