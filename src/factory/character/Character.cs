using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.factory;
using Necromation.factory.character.Actions;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;
using Necromation.sk;
using IInteractable = Necromation.interfaces.IInteractable;
using Resource = Necromation.Resource;

public partial class Character : Node2D
{
	public Vector2I MapPosition => Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	public PointLight2D Light => GetNode<PointLight2D>("%Light");
	private AudioStreamPlayer ClickAudio => GetNode<AudioStreamPlayer>("%ClickAudio");
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private readonly Inventory _inventory = new();
	
	/**************************************************************************
	 * Cursor and Building Placement                                          *
	 *************************************************************************/
	private Tween _cursorColorTween;
	private Sprite2D _cursorSprite;
	private Building _buildingInHand;
	private int _rotationDegrees;
	public IRotatable.BuildingOrientation Orientation => IRotatable.GetOrientationFromDegrees(_rotationDegrees);
	private string _selected;
	public string Selected
	{
		get => _selected;
		set => SetSelected(value);
	}

	/**************************************************************************
	 * Actions                                                                *
	 *************************************************************************/
	private readonly BuildAction _buildAction;
	private readonly InsertItemAction _insertItemAction;
	private readonly InteractAction _interactAction;
	private readonly MineAction _mineAction;
	private readonly RemoveBuildingAction _removeBuildingAction;
	private readonly RemoveItemAction _removeItemAction;
	
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	private const float Speed = 200;
	
	public Character()
	{
		_buildAction = new BuildAction(_inventory);
		_insertItemAction = new InsertItemAction(_inventory);
		_interactAction = new InteractAction(_inventory);
		_mineAction = new MineAction(_inventory);
		_removeBuildingAction = new RemoveBuildingAction(_inventory);
		_removeItemAction = new RemoveItemAction(_inventory);
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
		_cursorSprite = new Sprite2D();
		_cursorSprite.ZIndex = 100;
		_cursorSprite.Visible = false;
		_cursorSprite.Texture =  Database.Instance.GetTexture("Selection");
		Globals.FactoryScene.CallDeferred("add_child", _cursorSprite);
	}
	
	public override void _Process(double delta)
	{
		// Process mouseover
		if (Selected != null) SelectedPreview();
		else if (Globals.FactoryScene.TileMap.GetBuildingAtMouse() != null || Globals.FactoryScene.TileMap.GetResourceAtMouse() != null) MouseoverEntity();
		else _cursorSprite.Visible = false;

		if (Globals.FactoryScene.TileMap.GetEntity(MapPosition, FactoryTileMap.Building) is Belt belt) belt.MovePlayer(this, delta);
		
		// Return if a gui is open
		if (Globals.FactoryScene.Gui.IsAnyGuiOpen()) return;

		// Process button presses
		if (!_removeBuildingAction.IsRemoving)
		{
			var newPosition = Position;
			if (Input.IsActionPressed("right")) newPosition += new Vector2(Speed * (float)delta, 0);
			if (Input.IsActionPressed("left")) newPosition += new Vector2(-Speed * (float)delta, 0);
			if (Input.IsActionPressed("up")) newPosition += new Vector2(0, -Speed * (float)delta);
			if (Input.IsActionPressed("down")) newPosition += new Vector2(0, Speed * (float)delta);
			if (Globals.FactoryScene.TileMap.IsOnMap(Globals.FactoryScene.TileMap.GlobalToMap(newPosition))) Position = newPosition;
		}

		if (_mineAction.ShouldMine()) _mineAction.Mine();
		else _mineAction.Cancel();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Input.IsActionJustPressed("clear_selection")) QPick();

		var building = Globals.FactoryScene.TileMap.GetBuildingAtMouse();
		
		if (_insertItemAction.ShouldInsert(building)) _insertItemAction.Insert(building);
		else if (_removeItemAction.ShouldRemove(building)) _removeItemAction.Remove(building);
		else if (_buildAction.ShouldBuild()) _buildAction.Build();
		else if (_interactAction.ShouldInteract(building)) _interactAction.Interact(building);
		
		if (_removeBuildingAction.ShouldRemove()) _removeBuildingAction.RemoveBuilding(building);
		else _removeBuildingAction.CancelRemoval();
	}

	/******************************************************************
	 * Public Methods                                                 *
	 ******************************************************************/
	public void RotateSelection()
	{
		if (!Building.IsBuilding(_selected) || Building.GetBuilding(_selected, Orientation) is not IRotatable) return;
		_rotationDegrees += 90;
		if (_rotationDegrees == 360) _rotationDegrees = 0;
	}
	
	/******************************************************************
	 * Mouseover                                                      *
	 ******************************************************************/
	private void MouseoverEntity()
	{
		_cursorSprite.Visible = true;
		_cursorSprite.Modulate = Colors.White;
		_cursorSprite.Position = Globals.FactoryScene.TileMap.ToMap(GetGlobalMousePosition());

		if (_cursorColorTween != null) return;

		_cursorColorTween = GetTree().CreateTween();
		_cursorColorTween.TweenProperty(_cursorSprite, "modulate", Colors.Transparent, 0.5f);
		_cursorColorTween.TweenProperty(_cursorSprite, "modulate", Colors.White, 0.5f);
		_cursorColorTween.TweenCallback(Callable.From(() => _cursorColorTween = null));
	}

	private void SelectedPreview()
	{
		_cursorColorTween?.Kill();
		_cursorColorTween = null;
		_cursorSprite.Visible = true;
		_cursorSprite.RotationDegrees = _rotationDegrees;
		_cursorSprite.Position = Globals.FactoryScene.TileMap.ToMap(GetGlobalMousePosition());
		_cursorSprite.Modulate = Colors.White;
		
		if (!Building.IsBuilding(Selected)) return;

		if (_buildingInHand is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(_rotationDegrees);
		
		if (_buildingInHand.BuildingSize.X % 2 == 0) _cursorSprite.Position += new Vector2(16, 0);
		if (_buildingInHand.BuildingSize.Y % 2 == 0) _cursorSprite.Position += new Vector2(0, 16);
		
		_cursorSprite.Modulate = _buildingInHand.CanPlaceAt(GetGlobalMousePosition())
			? new Color(0, 1, 0, 0.5f)
			: new Color(1, 0, 0, 0.5f);
	}
	
	private void SetSelected(string value)
	{
		_selected = value;
		_cursorSprite.Texture = _selected == null
			? Database.Instance.GetTexture("Selection")
			: Database.Instance.GetTexture(_selected);

		_cursorSprite.Scale = !Building.IsBuilding(_selected)
			? new Vector2(32 / (float)_cursorSprite.Texture.GetWidth(), 32 / (float)_cursorSprite.Texture.GetHeight())
			: new Vector2(1, 1);

		if (Building.IsBuilding(Selected))
		{
			_buildingInHand = Building.GetBuilding(Selected, Orientation);
			if (_buildingInHand is not IRotatable) _rotationDegrees = 0;
		}
		else
		{
			_buildingInHand = null;
		}
	}
	
	/******************************************************************
	 * Player Actions                                                 *
	 ******************************************************************/
	private void QPick()
	{
		var cache = Selected;
		Selected = Globals.FactoryScene.TileMap.GetBuildingAtMouse() is Building building && building.ItemType != Selected && _inventory.Items.ContainsKey(building.ItemType)
			? building.ItemType
			: null;
		if (cache != Selected) ClickAudio.Play();
	}
	
	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		var dict =  new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", "Character" },
			{ "PosX", Position.X }, // Vector2 is not supported by JSON
			{ "PosY", Position.Y },
			{"Inventory0", _inventory.Save()}
		};
		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		Globals.Player.Position = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
		Globals.Player._inventory.Load((Godot.Collections.Dictionary)nodeData["Inventory0"]);
	}
}
