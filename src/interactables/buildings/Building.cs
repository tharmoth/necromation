using Godot;
using System;
using Necromation;
using Necromation.interactables.belts;

public abstract partial class Building : Node2D, BuildingTileMap.IBuilding, BuildingTileMap.IEntity, ProgressTracker.IProgress
{
	private readonly Sprite2D _sprite = new();

	protected Building()
	{
		AddChild(_sprite);
		
		var progress = new ProgressTracker();
		progress.NodeToTrack = this;
		progress.Size = new Vector2(128, 28);
		progress.Scale = new Vector2(0.25f, 0.25f);
		progress.Position = new Vector2(-16, 8);
		AddChild(progress);
	}

	public override void _Ready()
	{
		base._Ready();
		_sprite.Texture = Globals.Database.GetTexture(ItemType);

		GlobalPosition = Globals.TileMap.ToMap(GlobalPosition);
		
		if (BuildingSize.X % 2 == 0) GlobalPosition += new Vector2(16, 0);
		if (BuildingSize.Y % 2 == 0) GlobalPosition += new Vector2(0, 16);
		
		var center = Globals.TileMap.GlobalToMap(GlobalPosition);

		// Add the building to the tilemap based on the size of the building. the global position is the center of the
		// building. First we'll find the top left corner of the building.
		var topLeft = center - BuildingSize / 2;
		for (var x = 0; x < BuildingSize.X; x++)
		{
			for (var y = 0; y < BuildingSize.X; y++)
			{
				Globals.TileMap.AddEntity(topLeft + new Vector2I(x, y), this, BuildingTileMap.LayerNames.Buildings);
			}
		}
	}

	public bool CanPlaceAt(Vector2 position)
	{
		position = Globals.TileMap.ToMap(position);
		
		if (BuildingSize.X % 2 == 0) position += new Vector2(16, 0);
		if (BuildingSize.Y % 2 == 0) position += new Vector2(0, 16);
		
		var center = Globals.TileMap.GlobalToMap(position);
		
		var topLeft = center - BuildingSize / 2;
		for (var x = 0; x < BuildingSize.X; x++)
		{
			for (var y = 0; y < BuildingSize.X; y++)
			{
				var entity =
					Globals.TileMap.GetEntities(topLeft + new Vector2I(x, y), 
						BuildingTileMap.LayerNames.Buildings);
				if (entity != null) return false;
			}
		}

		return true;
	}

	private Tween _removeTween;
	public float RemovePercent;

	public void StartRemoval(Inventory to)
	{
		_removeTween?.Kill();
		_removeTween = GetTree().CreateTween();
		_removeTween.TweenProperty(this, "RemovePercent", 1.0f, 1.0f);
		_removeTween.TweenCallback(Callable.From(() => Remove(to)));
	}

	protected virtual void Remove(Inventory to)
	{
		if (this is ITransferTarget inputTarget)
		{
			foreach (var from in inputTarget.GetInventories())
			{
				Inventory.TransferAllTo(from, to);
			}
		}
		to.Insert(ItemType);
		Globals.TileMap.RemoveEntity(this);
	}
	
	public void CancelRemoval()
	{
		RemovePercent = 0;
		_removeTween?.Kill();
		_removeTween = null;
	}
	
	/******************************************************************
	 * Abstract methods                                               *
	 ******************************************************************/	
	public abstract Vector2I BuildingSize { get; }
	public abstract string ItemType { get; }
	public virtual float GetProgressPercent()
	{
		return 0;
	}

	/******************************************************************
	 * Building Factory                                               *
	 ******************************************************************/
	public static Building GetBuilding(string selectedItem)
	{
		return selectedItem switch
		{
			"Mine" => new Mine(),
			"Stone Furnace" => new Furnace(),
			"Stone Chest" => new StoneChest(),
			"Assembler" => new Assembler(),
			"Inserter" => new Inserter(),
			"Belt" => new Belt(),
			"Underground Belt" => new UndergroundBelt(),
			"Double Belt" => new DoubleBelt(),
			_ =>  throw new NotImplementedException()
		};
	}
	
	public static bool IsBuilding(string selectedItem)
	{
		return selectedItem switch
		{
			"Mine" => true,
			"Stone Furnace" => true,
			"Stone Chest" => true,
			"Assembler" => true,
			"Inserter" => true,
			"Belt" => true,
			"Underground Belt" => true,
			"Double Belt" => true,
			_ => false
		};
	}
}
