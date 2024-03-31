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
	private static readonly Texture2D SouthTexture = GD.Load<Texture2D>("res://res/sprites/Player-south.png");
	private static readonly Texture2D EastTexture = GD.Load<Texture2D>("res://res/sprites/Player-east.png");
	private static readonly Texture2D NorthTexture = GD.Load<Texture2D>("res://res/sprites/Player-north.png");
	
	public Vector2I MapPosition => Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	public PointLight2D Light => GetNode<PointLight2D>("%Light");
	private AudioStreamPlayer ClickAudio => GetNode<AudioStreamPlayer>("%ClickAudio");
	public Sprite2D Hands => GetNode<Sprite2D>("%Hands");
	public Sprite2D Body => GetNode<Sprite2D>("%Body");
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private readonly Inventory _inventory = new();
	
	/**************************************************************************
	 * Cursor and Building Placement                                          *
	 *************************************************************************/
	public int SelectionRotationDegrees;
	public IRotatable.BuildingOrientation SelectionOrientation => IRotatable.GetOrientationFromDegrees(SelectionRotationDegrees);
	public string Selected;

	/**************************************************************************
	 * Actions                                                                *
	 *************************************************************************/
	private readonly BuildBuildingAction _buildBuildingAction;
	private readonly RemoveBuildingAction _removeBuildingAction;
	private readonly InsertItemAction _insertItemAction;
	private readonly InteractAction _interactAction;
	private readonly MineAction _mineAction;
	private readonly RemoveItemAction _removeItemAction;
	private readonly DeconstructAction _deconstructAction = new();
	
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	private const float Speed = 1200;
	
	public Character()
	{
		_buildBuildingAction = new BuildBuildingAction(_inventory);
		_removeBuildingAction = new RemoveBuildingAction(_inventory);
		_insertItemAction = new InsertItemAction(_inventory);
		_interactAction = new InteractAction(_inventory);
		_mineAction = new MineAction(_inventory);
		_removeItemAction = new RemoveItemAction(_inventory);
		
		AddChild(_buildBuildingAction);
		AddChild(_removeBuildingAction);
		AddChild(_deconstructAction);
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
	}
	
	public override void _Process(double delta)
	{
		if (Globals.FactoryScene.TileMap.GetEntity(MapPosition, FactoryTileMap.Building) is Belt belt) belt.MovePlayer(this, delta);

		// Process button presses
		if (!_removeBuildingAction.IsRemoving)
		{
			
			var direction = new Vector2(
				Input.GetAxis("left", "right"), Input.GetAxis("up", "down")
			);
			
			var movement = direction.Normalized() * Speed * (float)delta;

			var newPositionX = GlobalPosition + new Vector2(movement.X, 0);
			if (Globals.FactoryScene.TileMap.IsOnMap(Globals.FactoryScene.TileMap.GlobalToMap(newPositionX)) &&
			    Globals.FactoryScene.TileMap.GetEntity(newPositionX, FactoryTileMap.Building) is null or Belt or Inserter) GlobalPosition = newPositionX;
			
			var newPositionY = GlobalPosition + new Vector2(0, movement.Y);
			if (Globals.FactoryScene.TileMap.IsOnMap(Globals.FactoryScene.TileMap.GlobalToMap(newPositionY)) &&
			    Globals.FactoryScene.TileMap.GetEntity(newPositionY, FactoryTileMap.Building) is null or Belt or Inserter) GlobalPosition = newPositionY;

			
			if (direction.Length() > 0)
			{
				Body.Texture = Mathf.Abs(direction.X) > 0 ? EastTexture : direction.Y > 0 ? SouthTexture : NorthTexture;
				Body.FlipH = direction.X switch
				{
					< 0 => true,
					> 0 => false,
					_ => Body.FlipH
				};
			}
			else
			{
				Body.Texture = SouthTexture;
			}
		}
		
		// Return if a gui is open
		if (Globals.FactoryScene.Gui.IsAnyGuiOpen()) return;

		// Mine action should be here as it should repeat without inputevents.
		if (_mineAction.ShouldMine()) _mineAction.Mine();
		else _mineAction.Cancel();
		
		ProcessInput();
	}

	private static int counter = 0;
	
	private void ProcessInput()
	{
		if (_motionEventHandled) return;

		var building = Globals.FactoryScene.TileMap.GetBuildingAtMouse();
		
		// Cursor Input Actions
		if (_insertItemAction.ShouldInsert(building)) _insertItemAction.Insert(building);
		else if (_removeItemAction.ShouldRemove(building)) _removeItemAction.Remove(building);
		else if (_buildBuildingAction.ShouldBuild()) _buildBuildingAction.Build();
		else if (_interactAction.ShouldInteract(building)) _interactAction.Interact(building);
		
		// Always Cancel removal if not removing
		if (_removeBuildingAction.ShouldRemove()) _removeBuildingAction.RemoveBuilding(building);
		else _removeBuildingAction.CancelRemoval();
	}
	
	private bool _motionEventHandled = false;
	
	public override void _Input(InputEvent @event)
	{
		ProcessInput();

		if (@event is not InputEventMouseMotion) return;
		_motionEventHandled = true;
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Input.IsActionJustPressed("clear_selection")) QPick();
		
		if (@event is not InputEventMouseMotion) return;
		_motionEventHandled = false;
	}

	/******************************************************************
	 * Public Methods                                                 *
	 ******************************************************************/
	public void RotateSelection()
	{
		if (!Building.IsBuilding(Selected) || Building.GetBuilding(Selected, SelectionOrientation) is not IRotatable) return;
		SelectionRotationDegrees += 90;
		if (SelectionRotationDegrees == 360) SelectionRotationDegrees = 0;
	}
	
	/******************************************************************
	 * Player Actions                                                 *
	 ******************************************************************/
	private void QPick()
	{
		var cache = Selected;
		var buildingAtMouse = Globals.FactoryScene.TileMap.GetBuildingAtMouse();
		if (buildingAtMouse is IRotatable rotatable) SelectionRotationDegrees = IRotatable.GetDegreesFromOrientation(rotatable.Orientation);
		Selected = buildingAtMouse != null && buildingAtMouse.ItemType != Selected && _inventory.Items.ContainsKey(buildingAtMouse.ItemType)
			? buildingAtMouse.ItemType
			: null;
		if (cache != Selected) ClickAudio.Play();
	}
	
	#region Save/Load
	/******************************************************************
	 * Save/Load                                                      *
	 ******************************************************************/
	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		var dict =  new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", "Character" },
			{ "PosX", Position.X },
			{ "PosY", Position.Y },
			{ "PlayerInventory", _inventory.Save() }
		};
		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		Globals.Player.Position = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
		Globals.Player._inventory.Load((Godot.Collections.Dictionary)nodeData["PlayerInventory"]);
	}
	#endregion
}
