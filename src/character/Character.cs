using Godot;
using System;
using Necromation;
using Necromation.character;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;
using Belt = Necromation.interactables.belts.Belt;
using IInteractable = Necromation.interfaces.IInteractable;

public partial class Character : Node2D
{
	private const float Speed = 100;
	
	private Inventory _inventory = new();
	private Tween _tween;
	private Sprite2D _sprite;
	
	private Interactable _resource;
	private int _rotationDegrees;

	private String _selected;
	public String Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			_sprite.Texture = _selected == null
				? GD.Load<Texture2D>("res://res/sprites/selection.png")
				: Globals.Database.GetTexture(_selected);
		}
	}
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Globals.PlayerInventory = _inventory;
		Globals.Player = this;
	}

	public override void _Ready()
	{
		AddToGroup("player");
		_inventory.Insert("Stone", 500);
		_inventory.Insert("Mine", 3);
		_inventory.Insert("Stone Furnace", 100);
		_inventory.Insert("Stone Chest", 100);
		_inventory.Insert("Inserter", 100);
		_inventory.Insert("Assembler", 3);
		_inventory.Insert("Belt", 100);
		_inventory.Insert("Underground Belt", 100);
		_sprite = new Sprite2D();
		_sprite.ZIndex = 2;
		_sprite.Visible = false;
		_sprite.Texture = GD.Load<Texture2D>("res://res/sprites/selection.png");
		GetTree().Root.CallDeferred("add_child", _sprite);
	}
	
	public override void _Process(double delta)
	{
		if (GUI.Instance.CrafterGui.Visible) return;
		if (GUI.Instance.Popup.Visible) return;
		if (GUI.Instance.ContainerGui.Visible) return;
		
		// Move the character at speed
		if (Input.IsActionPressed("right")) Position += new Vector2(Speed * (float)delta, 0);
		if (Input.IsActionPressed("left")) Position += new Vector2(-Speed * (float)delta, 0);
		if (Input.IsActionPressed("up")) Position += new Vector2(0, -Speed * (float)delta);
		if (Input.IsActionPressed("down")) Position += new Vector2(0, Speed * (float)delta);
		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Selected == null) _rotationDegrees = 0;
		if (Input.IsActionPressed("close_gui")) Selected = null;

		ProcessCursor();
	}
	
	public void RotateSelection()
	{
		_rotationDegrees += 90;
		if (_rotationDegrees == 360) _rotationDegrees = 0;
	}

	private void ProcessCursor()
	{
		if (Input.IsMouseButtonPressed(MouseButton.Right) && GetResorceAtMouse() is Interactable resource)
		{
			if (resource == _resource && !_resource.CanInteract()) return;
			_resource?.Cancel();
			_resource = resource;
			_resource.Interact();
		}
		else
		{
			_resource?.Cancel();
			_resource = null;
		}

		if (Input.IsActionJustPressed("left_click") && GetBuildingAtMouse() is IInteractable interactable) interactable.Interact();
		
		if (Selected != null) ProcessCursorSelected();
		else if (Globals.TileMap.GetEntities(GetGlobalMousePosition()).Count > 0) ProcessCursorMouseover();
		else _sprite.Visible = false;
		
		if (Input.IsMouseButtonPressed(MouseButton.Left)) LeftMouseButtonPressed();
		if (Input.IsMouseButtonPressed(MouseButton.Right)) RightMouseButtonPressed();
		if (Input.IsMouseButtonPressed(MouseButton.Right)) Selected = null;
	}

	private void ProcessCursorMouseover()
	{
		_sprite.Visible = true;
		_sprite.Modulate = Colors.White;
		_sprite.Position = Globals.TileMap.ToMap(GetGlobalMousePosition());

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
		_sprite.RotationDegrees = _rotationDegrees;
		_sprite.Modulate = GetMouseoverColor();
		_sprite.Position = Globals.TileMap.ToMap(GetGlobalMousePosition());
		
		//TODO: making buildings every tick seems like a bad idea.
		if (Building.IsBuilding(Selected))
		{
			var building = Building.GetBuilding(Selected);
			if (building.BuildingSize.X % 2 == 0) _sprite.Position += new Vector2(16, 0);
			if (building.BuildingSize.Y % 2 == 0) _sprite.Position += new Vector2(0, 16);
		}
	}

	private Color GetMouseoverColor()
	{
		var building = GetBuildingAtMouse();
		if (building is Belt && _selected is Belt) return Colors.Green;
		return GetBuildingAtMouse() == null ? Colors.Green : Colors.Red;
	}

	private void LeftMouseButtonPressed()
	{
		if (Building.IsBuilding(Selected)) Build(Building.GetBuilding(Selected));
	}
	
	private void RightMouseButtonPressed()
	{
		RemoveBuilding();
	}

	private void Build(Building building)
	{
		if (!_inventory.Items.ContainsKey(building.ItemType))
		{
			Selected = null;
			return;
		}
		
		var position = GetGlobalMousePosition();
		if (!Globals.TileMap.IsEmpty(position, BuildingTileMap.LayerNames.Buildings)) return;
		
		_inventory.Remove(building.ItemType);

		if (building is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(_rotationDegrees);

		building.GlobalPosition = position;
		GetTree().Root.AddChild(building);
		
		if(!_inventory.Items.ContainsKey(building.ItemType)) Selected = null;
	}
	
	private void RemoveBuilding()
	{
		var building = GetBuildingAtMouse();
		if (building is not BuildingTileMap.IBuilding buildingEntity) return;
		if (building is Inserter.ITransferTarget inputTarget)
		{
			inputTarget.GetInputInventory().TransferAllTo(_inventory);
			inputTarget.GetOutputInventory().TransferAllTo(_inventory);
		}
		_inventory.Insert(buildingEntity.ItemType);
		
		Globals.TileMap.RemoveEntity(building);
	}

	private BuildingTileMap.IEntity GetBuildingAtMouse()
	{
		return Globals.TileMap.GetEntities(GetGlobalMousePosition(), BuildingTileMap.LayerNames.Buildings);
	}
	
	private BuildingTileMap.IEntity GetResorceAtMouse()
	{
		return Globals.TileMap.GetEntities(GetGlobalMousePosition(), BuildingTileMap.LayerNames.Resources);
	}
}
