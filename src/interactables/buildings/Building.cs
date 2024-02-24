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

	private float _removePercent;
	public float RemovePercent
	{
		get => _removePercent;
		set
		{
			_removePercent = value;
			GUI.Instance.SetProgress(value);
		}
	}
	
	private AudioStreamPlayer2D _audio = new();

	protected Building()
	{
		AddChild(Sprite);
		AddChild(_audio);
		
		_audio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_foley_boots_wellington_rubber_pair_set_down_grass_001_105602.mp3");
		
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

		var positions = GetOccupiedPositions(GlobalPosition);
		positions.ForEach(pos => Globals.TileMap.AddEntity(pos, this, BuildingTileMap.Building));
		
		_audio.Play();
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
		_removeTween.TweenProperty(this, "RemovePercent", 1.0f, .333f);
		_removeTween.TweenCallback(Callable.From(() => Remove(to)));
	}
	
		
	public void CancelRemoval()
	{
		RemovePercent = 0;
		_removeTween?.Kill();
		_removeTween = null;
	}
	
	public List<Vector2I> GetOccupiedPositions(Vector2 position)
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

	public virtual void Remove(Inventory to)
	{
		_audio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_foley_metal_med_heavy_pole_crowbar_pick_up_from_concrete_003_58640.mp3");
		if (this is ITransferTarget inputTarget)
		{
			foreach (var from in inputTarget.GetInventories())
			{
				Inventory.TransferAllTo(from, to);
			}
		}
		to.Insert(ItemType);
		Globals.TileMap.RemoveEntity(this);
		RemovePercent = 100;
		Sprite.Visible = false;
		_audio.Play();
		_audio.Finished += QueueFree;
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
			"Long Inserter" => new Inserter(2),
			"Belt" => new Belt(),
			"Underground Belt" => new UndergroundBelt(),
			"Research Lab" => new ResearchLab(),
			"Barracks" => new Barracks(),
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
			"Barracks" => true,
			"Long Inserter" => true,
			_ => false
		};
	}
}
