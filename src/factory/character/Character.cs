using System.Linq;
using Godot;
using Necromation;
using Necromation.factory;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;
using Necromation.sk;
using IInteractable = Necromation.interfaces.IInteractable;
using Resource = Necromation.Resource;

public partial class Character : Node2D
{
	public IRotatable.BuildingOrientation Orientation => IRotatable.GetOrientationFromDegrees(_rotationDegrees);
	public Vector2I MapPosition => Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
	public PointLight2D Light => GetNode<PointLight2D>("%Light");
	private AudioStreamPlayer _clickAudio => GetNode<AudioStreamPlayer>("%ClickAudio");
	
	private const float Speed = 200;
	private readonly Inventory _inventory = new();
	private Tween _tween;
	private Sprite2D _sprite;
	
	private Resource _resource;
	private Building _buildingBeingRemoved;
	private Building _buildingInHand;

	private int _rotationDegrees;
	private string _selected;
	public string Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			_sprite.Texture = _selected == null
				?  Database.Instance.GetTexture("Selection")
				: Database.Instance.GetTexture(_selected);
			
			_sprite.Scale = !Building.IsBuilding(_selected) ? 
				new Vector2(32 / (float)_sprite.Texture.GetWidth(), 32 / (float)_sprite.Texture.GetHeight()) 
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
	}
	
	private Tween _removeTween;

	private float _removePercent;
	public float RemovePercent
	{
		get => _removePercent;
		set
		{
			_removePercent = value;
			Globals.FactoryScene.Gui.SetProgress(value);
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
		// Extra insurance against breaking builds.
		if (OS.IsDebugBuild())
		{
			Database.Instance.Recipes
				.Select(recipe => recipe.Products.First().Key)
				.ToList()
				.ForEach(item => Globals.PlayerInventory.Insert(item, 100));
			
			_inventory.Insert("Coal Ore", 1000);
			_inventory.Insert("Bone Fragments", 1000);
		}
		
		
		AddToGroup("player");
		_sprite = new Sprite2D();
		_sprite.ZIndex = 100;
		_sprite.Visible = false;
		_sprite.Texture =  Database.Instance.GetTexture("Selection");
		Globals.FactoryScene.CallDeferred("add_child", _sprite);
	}
	
	public override void _Process(double delta)
	{
		// Process mouseover
		if (Selected != null) SelectedPreview();
		else if (Globals.FactoryScene.TileMap.GetBuildingAtMouse() != null || Globals.FactoryScene.TileMap.GetResourceAtMouse() != null) MouseoverEntity();
		else _sprite.Visible = false;

		if (Globals.FactoryScene.TileMap.GetEntity(MapPosition, FactoryTileMap.Building) is Belt belt) belt.MovePlayer(this, delta);
		
		// Return if a gui is open
		if (Globals.FactoryScene.Gui.IsAnyGuiOpen()) return;

		// Process button presses
		if (_buildingBeingRemoved == null)
		{
			var newPosition = Position;
			if (Input.IsActionPressed("right")) newPosition += new Vector2(Speed * (float)delta, 0);
			if (Input.IsActionPressed("left")) newPosition += new Vector2(-Speed * (float)delta, 0);
			if (Input.IsActionPressed("up")) newPosition += new Vector2(0, -Speed * (float)delta);
			if (Input.IsActionPressed("down")) newPosition += new Vector2(0, Speed * (float)delta);
			if (Globals.FactoryScene.TileMap.IsOnMap(Globals.FactoryScene.TileMap.GlobalToMap(newPosition))) Position = newPosition;
		}

		if (Input.IsMouseButtonPressed(MouseButton.Right) && Globals.FactoryScene.TileMap.GetBuildingAtMouse() == null) Mine();
		else _resource?.Cancel();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Input.IsActionJustPressed("rotate")) RotateSelection();
		if (Input.IsActionJustPressed("clear_selection")) QPick();

		if (!string.IsNullOrEmpty(Selected) && Input.IsMouseButtonPressed(MouseButton.Left) && Input.IsKeyPressed(Key.Ctrl) &&
		    Globals.FactoryScene.TileMap.GetBuildingAtMouse() is ITransferTarget transfer && transfer.GetMaxTransferAmount(Selected) > 0)
		{
			InsertToBuilding(transfer);
		}
		else if (string.IsNullOrEmpty(Selected) && Input.IsMouseButtonPressed(MouseButton.Left) && Input.IsKeyPressed(Key.Ctrl) &&
		    Globals.FactoryScene.TileMap.GetBuildingAtMouse() is ITransferTarget transfer2 && transfer2.GetOutputInventory().CountAllItems() > 0)
		{
			RemoveFromBuilding(transfer2);
		} 
		else if (Input.IsMouseButtonPressed(MouseButton.Left) && Building.IsBuilding(Selected)) Build();
		else if (Input.IsActionJustPressed("left_click") 
		         && !Building.IsBuilding(Selected)
		         && !Input.IsKeyPressed(Key.Ctrl) 
		         && Globals.FactoryScene.TileMap.GetBuildingAtMouse() is IInteractable interactable) interactable.Interact(_inventory);
		
		if (Input.IsMouseButtonPressed(MouseButton.Right)) RemoveBuilding();
		else CancelRemoval();
		
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
		_sprite.Visible = true;
		_sprite.Modulate = Colors.White;
		_sprite.Position = Globals.FactoryScene.TileMap.ToMap(GetGlobalMousePosition());

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
		_sprite.Position = Globals.FactoryScene.TileMap.ToMap(GetGlobalMousePosition());
		_sprite.Modulate = Colors.White;
		
		if (!Building.IsBuilding(Selected)) return;

		if (_buildingInHand is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(_rotationDegrees);
		
		if (_buildingInHand.BuildingSize.X % 2 == 0) _sprite.Position += new Vector2(16, 0);
		if (_buildingInHand.BuildingSize.Y % 2 == 0) _sprite.Position += new Vector2(0, 16);
		
		_sprite.Modulate = _buildingInHand.CanPlaceAt(GetGlobalMousePosition())
			? new Color(0, 1, 0, 0.5f)
			: new Color(1, 0, 0, 0.5f);
	}


	/******************************************************************
	 * Player Actions                                                 *
	 ******************************************************************/
	private void Build()
	{
		var building = _buildingInHand;
		if (!_inventory.Items.ContainsKey(building.ItemType))
		{
			Selected = null;
			return;
		}
		
		var position = GetGlobalMousePosition();
		if (!building.CanPlaceAt(position)) return;
		// Remove any buildings that are in the way. This should probably only happen for IRotatable buildings.
		building.GetOccupiedPositions(position)
			.Select(pos => Globals.FactoryScene.TileMap.GetEntity(pos, FactoryTileMap.Building))
			.Select(entity => entity as Building)
			.Where(entity => entity != null)
			.Distinct()
			.ToList()
			.ForEach(bldg => bldg.Remove(_inventory));
		
		_inventory.Remove(building.ItemType);

		Globals.BuildingManager.AddBuilding(building, position);

		if(!_inventory.Items.ContainsKey(building.ItemType)) Selected = null;
		else _buildingInHand = Building.GetBuilding(Selected, Orientation);
	}

	private void RemoveBuilding()
	{
		var entity = Globals.FactoryScene.TileMap.GetBuildingAtMouse();
		if (entity != _buildingBeingRemoved) CancelRemoval();
		if (entity == _buildingBeingRemoved) return;
		if (entity is not Building building) return;
		_buildingBeingRemoved = building;
		_removeTween?.Kill();
		_removeTween = Globals.Tree.CreateTween();
		_removeTween.TweenProperty(this, "RemovePercent", 1.0f, .333f);
		_removeTween.TweenCallback(Callable.From(() =>
		{
			RemovePercent = 100;
			_buildingBeingRemoved.Remove(_inventory);
		}));
	}

	private void CancelRemoval()
	{
		RemovePercent = 0;
		_removeTween?.Kill();
		_removeTween = null;
		_buildingBeingRemoved = null;
	}
	
	private void QPick()
	{
		var cache = Selected;
		Selected = Globals.FactoryScene.TileMap.GetBuildingAtMouse() is Building building && building.ItemType != Selected && _inventory.Items.ContainsKey(building.ItemType)
			? building.ItemType
			: null;
		if (cache != Selected) _clickAudio.Play();
	}

	private void Mine()
	{
		if (Globals.FactoryScene.TileMap.GetResourceAtMouse() is Resource resource)
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

	private void InsertToBuilding(ITransferTarget transfer)
	{
		var count = 0;
		
		count = Mathf.Min(transfer.GetMaxTransferAmount(Selected), _inventory.CountItem(Selected));
		Inventory.TransferItem(_inventory, transfer.GetInputInventory(), Selected, count);

		var remaining = _inventory.CountItem(Selected);

		SKFloatingLabel.Show("-" + count + " " + Selected + " (" + remaining + ")" , ((Building)transfer).GlobalPosition);
			
		if (remaining == 0) Selected = "";
		
		MusicManager.PlayCraft();
	}
	
	private void RemoveFromBuilding(ITransferTarget transfer)
	{

		var from = transfer.GetOutputInventory();

		var index = 0;
		foreach (var item in from.GetItems())
		{
			var count = from.CountItem(item);
			Inventory.TransferItem(from, _inventory, item, count);
			var remaining = _inventory.CountItem(item);
			SKFloatingLabel.Show("+" + count + " " + item + " (" + remaining + ")", ((Building)transfer).GlobalPosition + new Vector2(0, index++ * 20));
		}

		MusicManager.PlayCraft();
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
