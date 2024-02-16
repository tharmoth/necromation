using Godot;
using System;
using Necromation;
using Necromation.character;
using Necromation.interactables.belts;
using Belt = Necromation.interactables.belts.Belt;
using IInteractable = Necromation.interfaces.IInteractable;

public partial class Character : Node2D
{
	Inventory _inventory = new();
	private Object _selected;
	private Tween _tween;
	private Sprite2D _sprite;
	private float _speed = 100;
	private Interactable _resource;
	private int rotationDegrees = 0;

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
		if (Input.IsActionPressed("right")) Position += new Vector2(_speed * (float)delta, 0);
		if (Input.IsActionPressed("left")) Position += new Vector2(-_speed * (float)delta, 0);
		if (Input.IsActionPressed("up")) Position += new Vector2(0, -_speed * (float)delta);
		if (Input.IsActionPressed("down")) Position += new Vector2(0, _speed * (float)delta);
		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Selected == null) rotationDegrees = 0;
		if (Input.IsActionPressed("close_gui")) Selected = null;

		ProcessCursor();
	}
	
	private void RotateSelection()
	{
		rotationDegrees += 90;
		if (rotationDegrees == 360) rotationDegrees = 0;
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
		_sprite.RotationDegrees = rotationDegrees;
		_sprite.Modulate = GetMouseoverColor();
		_sprite.Position = Globals.TileMap.ToMap(GetGlobalMousePosition());
	}

	private Color GetMouseoverColor()
	{
		var building = GetBuildingAtMouse();
		if (building is Belt && _selected is string selectedString && selectedString == "Belt") return Colors.Green;
		return GetBuildingAtMouse() == null ? Colors.Green : Colors.Red;
	}

	private void LeftMouseButtonPressed()
	{
		if (Selected is string selectedString) Build(selectedString);
	}
	
	private void RightMouseButtonPressed()
	{
		RemoveBuilding();
	}

	private void Build(string selectedItem)
	{
		if (!_inventory.Items.ContainsKey(selectedItem))
		{
			Selected = null;
			return;
		}
		
		var position = GetGlobalMousePosition();
		if (!Globals.TileMap.IsEmpty(position, BuildingTileMap.LayerNames.Buildings)) return;
		
		_inventory.Remove(selectedItem);

		Building building = selectedItem switch
		{
			"Mine" => new Mine(),
			"Stone Furnace" => new Furnace(),
			"Stone Chest" => new StoneChest(),
			"Assembler" => new Assembler(),
			"Inserter" => new Inserter(rotationDegrees),
			"Belt" => new Belt(rotationDegrees),
			"Underground Belt" => new UndergroundBelt(rotationDegrees),
			_ =>  throw new NotImplementedException()
		};
		
		Globals.TileMap.AddEntity(position, building, BuildingTileMap.LayerNames.Buildings);

		// TODO: I don't like having this exception.
		if (building is UndergroundBelt)
		{
			RotateSelection();
			RotateSelection();
		}
		
		if(!_inventory.Items.ContainsKey(selectedItem)) Selected = null;
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
