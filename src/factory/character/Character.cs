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
	public int SelectionRotationDegrees;
	public IRotatable.BuildingOrientation SelectionOrientation => IRotatable.GetOrientationFromDegrees(SelectionRotationDegrees);
	public string Selected;

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
	}
	
	public override void _Process(double delta)
	{
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

		// Mine action should be here as it should repeat without inputevents.
		if (_mineAction.ShouldMine()) _mineAction.Mine();
		else _mineAction.Cancel();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		// Gui input actions
		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Input.IsActionJustPressed("clear_selection")) QPick();

		var building = Globals.FactoryScene.TileMap.GetBuildingAtMouse();
		
		// Cursor Input Actions
		if (_insertItemAction.ShouldInsert(building)) _insertItemAction.Insert(building);
		else if (_removeItemAction.ShouldRemove(building)) _removeItemAction.Remove(building);
		else if (_buildAction.ShouldBuild()) _buildAction.Build();
		else if (_interactAction.ShouldInteract(building)) _interactAction.Interact(building);
		
		// Always Cancel removal if not removing
		if (_removeBuildingAction.ShouldRemove()) _removeBuildingAction.RemoveBuilding(building);
		else _removeBuildingAction.CancelRemoval();
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
		Selected = Globals.FactoryScene.TileMap.GetBuildingAtMouse() is Building building && building.ItemType != Selected && _inventory.Items.ContainsKey(building.ItemType)
			? building.ItemType
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
