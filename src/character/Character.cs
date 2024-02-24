using System.Linq;
using Godot;
using Necromation;
using Necromation.interactables.interfaces;
using IInteractable = Necromation.interfaces.IInteractable;

public partial class Character : Node2D
{
	private const float Speed = 200;
	private readonly Inventory _inventory = new();
	private Tween _tween;
	private Sprite2D _sprite;
	
	private Collectable _resource;
	private Building _buildingBeingRemoved;

	private int _rotationDegrees;
	private string _selected;
	public string Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			_sprite.Texture = _selected == null
				? GD.Load<Texture2D>("res://res/sprites/selection.png")
				: Globals.Database.GetTexture(_selected);
			
			if (_selected == null) _rotationDegrees = 0;
		}
	}
	
	public override void _EnterTree()
	{
		base._EnterTree();
		Globals.Player = this;
		Globals.PlayerInventory = _inventory;
	}

	public override void _Ready()
	{
		AddToGroup("player");
		
		Globals.Database.Recipes
			.Select(recipe => recipe.Products.First().Key)
			.ToList()
			.ForEach(item => _inventory.Insert(item, 100));

		_sprite = new Sprite2D();
		_sprite.ZIndex = 2;
		_sprite.Visible = false;
		_sprite.Texture = GD.Load<Texture2D>("res://res/sprites/selection.png");
		Globals.FactoryScene.CallDeferred("add_child", _sprite);
	}
	
	public override void _Process(double delta)
	{
		// Return if a gui is open
		if (GUI.Instance.IsAnyGuiOpen()) return;

		// Process button presses
		if (_buildingBeingRemoved == null)
		{
			if (Input.IsActionPressed("right")) Position += new Vector2(Speed * (float)delta, 0);
			if (Input.IsActionPressed("left")) Position += new Vector2(-Speed * (float)delta, 0);
			if (Input.IsActionPressed("up")) Position += new Vector2(0, -Speed * (float)delta);
			if (Input.IsActionPressed("down")) Position += new Vector2(0, Speed * (float)delta);
		}

		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Input.IsActionPressed("close_gui") || Input.IsActionPressed("clear_selection")) Selected = Globals.TileMap.GetBuildingAtMouse() is Building building ? building.ItemType : null;
		if (Input.IsActionJustPressed("left_click") && Globals.TileMap.GetBuildingAtMouse() is IInteractable interactable) interactable.Interact(_inventory);
		if (Input.IsMouseButtonPressed(MouseButton.Left) && Building.IsBuilding(Selected)) Build(Building.GetBuilding(Selected));
		if (Input.IsMouseButtonPressed(MouseButton.Right)) RemoveBuilding();
		else CancelRemoval();
		if (Input.IsMouseButtonPressed(MouseButton.Right)) Mine();
		else _resource?.Cancel();

		// Process mouseover
		if (Selected != null) SelectedPreview();
		else if (Globals.TileMap.GetEntity(GetGlobalMousePosition(), BuildingTileMap.Building) != null) MouseoverEntity();
		else _sprite.Visible = false;
	}

	/******************************************************************
	 * Public Methods                                                 *
	 ******************************************************************/
	public void RotateSelection()
	{
		_rotationDegrees += 90;
		if (_rotationDegrees == 360) _rotationDegrees = 0;
	}
	
	/******************************************************************
	 * Mouseover                                                      *
	 ******************************************************************/
	private void MouseoverEntity()
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

	private void SelectedPreview()
	{
		_tween?.Kill();
		_tween = null;
		_sprite.Visible = true;
		_sprite.RotationDegrees = _rotationDegrees;
		_sprite.Position = Globals.TileMap.ToMap(GetGlobalMousePosition());
		
		if (!Building.IsBuilding(Selected)) return;
		
		//TODO: making buildings every tick seems like a bad idea.
		var buildingInHand = Building.GetBuilding(Selected);
		
		if (buildingInHand is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(_rotationDegrees);
		
		if (buildingInHand.BuildingSize.X % 2 == 0) _sprite.Position += new Vector2(16, 0);
		if (buildingInHand.BuildingSize.Y % 2 == 0) _sprite.Position += new Vector2(0, 16);
		
		_sprite.Modulate = buildingInHand.CanPlaceAt(GetGlobalMousePosition())
			? Colors.Green
			: Colors.Red;
	}


	/******************************************************************
	 * Player Actions                                                 *
	 ******************************************************************/
	private void Build(Building building)
	{
		if (!_inventory.Items.ContainsKey(building.ItemType))
		{
			Selected = null;
			return;
		}
		
		var position = GetGlobalMousePosition();
		if (building is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(_rotationDegrees);
		
		if (!building.CanPlaceAt(position)) return;

		// Remove any buildings that are in the way. This should probably only happen for IRotatable buildings.
		building.GetOccupiedPositions(position)
			.Select(pos => Globals.TileMap.GetEntity(pos, BuildingTileMap.Building))
			.Select(entity => entity as Building)
			.Where(entity => entity != null)
			.Distinct()
			.ToList()
			.ForEach(bldg => bldg.Remove(_inventory));
		
		_inventory.Remove(building.ItemType);


		building.GlobalPosition = position;
		Globals.FactoryScene.AddChild(building);
		
		if(!_inventory.Items.ContainsKey(building.ItemType)) Selected = null;
	}

	private void RemoveBuilding()
	{
		var entity = Globals.TileMap.GetBuildingAtMouse();
		if (entity != _buildingBeingRemoved) CancelRemoval();
		if (entity == _buildingBeingRemoved) return;
		if (entity is not Building building) return;
		_buildingBeingRemoved = building;
		building.StartRemoval(_inventory);
	}
	
	private void CancelRemoval()
	{
		_buildingBeingRemoved?.CancelRemoval();
		_buildingBeingRemoved = null;
	}

	private void Mine()
	{
		if (Globals.TileMap.GetResourceAtMouse() is Collectable resource)
		{
			if (resource == _resource && !_resource.CanInteract()) return;
			_resource?.Cancel();
			_resource = resource;
			_resource.Interact(_inventory);
		}
		else
		{
			_resource?.Cancel();
			_resource = null;
		}
	}
}
