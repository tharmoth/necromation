using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.interactables.belts;

public abstract partial class Building : Node2D, BuildingTileMap.IEntity, ProgressTracker.IProgress
{
	protected readonly Sprite2D Sprite = new();
	private Tween _removeTween;
	public float RemovePercent;

	protected Building()
	{
		AddChild(Sprite);
		
		var progress = new ProgressTracker();
		progress.NodeToTrack = this;
		progress.Size = new Vector2(128, 28);
		progress.Scale = new Vector2(0.25f, 0.25f);
		progress.Position = new Vector2(-16, 8);
		// AddChild(progress);
	}

	public override void _Ready()
	{
		base._Ready();
		Sprite.Texture = Globals.Database.GetTexture(ItemType);
		GlobalPosition = Globals.TileMap.ToMap(GlobalPosition);
		if (BuildingSize.X % 2 == 0) Sprite.GlobalPosition += new Vector2(16, 0);
		if (BuildingSize.Y % 2 == 0) Sprite.GlobalPosition += new Vector2(0, 16);

		// If the building is placed on top of another building, remove the other building. This should only happen for
		// IRotateables as other buildings should not be able to be placed on top of each other.
		var positions = GetOccupiedPositions(GlobalPosition);
		positions.Select(pos => Globals.TileMap.GetEntities(pos, BuildingTileMap.Building))
			.Select(entity => entity as Building)
			.Where(entity => entity != null)
			.Distinct()
			.ToList()
			.ForEach(building => building.Remove(Globals.PlayerInventory));
		
		positions.ForEach(pos => Globals.TileMap.AddEntity(pos, this, BuildingTileMap.Building));
	}
	
	/******************************************************************
	 * Public Methods                                                 *
	 ******************************************************************/
	public virtual bool CanPlaceAt(Vector2 position)
	{
		return GetOccupiedPositions(position).All(Globals.TileMap.IsBuildable);
	}

	public void StartRemoval(Inventory to)
	{
		_removeTween?.Kill();
		_removeTween = GetTree().CreateTween();
		_removeTween.TweenProperty(this, "RemovePercent", 1.0f, 1.0f);
		_removeTween.TweenCallback(Callable.From(() => Remove(to)));
	}
	
		
	public void CancelRemoval()
	{
		RemovePercent = 0;
		_removeTween?.Kill();
		_removeTween = null;
	}

	/******************************************************************
	 * Protected Methods                                              *
	 ******************************************************************/
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

	protected List<Vector2I> GetOccupiedPositions(Vector2 position)
	{
		position = Globals.TileMap.ToMap(position);
		if (BuildingSize.X % 2 == 0) position += new Vector2(16, 0);
		if (BuildingSize.Y % 2 == 0) position += new Vector2(0, 16);
		var topLeft = Globals.TileMap.GlobalToMap(position) - BuildingSize / 2;

		var positions = (from x in Enumerable.Range(0, BuildingSize.X)
			from y in Enumerable.Range(0, BuildingSize.Y)
			select topLeft + new Vector2I(x, y)).ToList();
		return positions;
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
			"Research Lab" => new ResearchLab(),
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
			"Research Lab" => true,
			_ => false
		};
	}
}
