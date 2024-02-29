using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;

public abstract partial class Building : Node2D, BuildingTileMap.IEntity, ProgressTracker.IProgress
{
	public Vector2I MapPosition => Globals.TileMap.GlobalToMap(GlobalPosition);
	protected readonly Sprite2D Sprite = new();
	private Tween _removeTween;

	private float _removePercent;
	public float RemovePercent
	{
		get => _removePercent;
		set
		{
			_removePercent = value;
			FactoryGUI.Instance.SetProgress(value);
		}
	}
	
	private AudioStreamPlayer2D _audio = new();
	private ProgressTracker _progress = new();

	protected Building()
	{
		AddChild(Sprite);
		AddChild(_audio);
		
		_audio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_foley_boots_wellington_rubber_pair_set_down_grass_001_105602.mp3");
		
		_progress.NodeToTrack = this;
		_progress.Scale = new Vector2(0.25f, 0.25f);
		AddChild(_progress);
		
		AddToGroup("Persist");
	}

	public override void _Ready()
	{
		base._Ready();
		if (BuildingSize.X % 2 != 0)
		{
			_progress.Size = new Vector2(BuildingTileMap.TileSize * BuildingSize.X * 4 - 40, 28);
			_progress.Position -= new Vector2(BuildingTileMap.TileSize * BuildingSize.X / 2.0f - 5, -BuildingTileMap.TileSize * BuildingSize.X / 2.0f + 28 / 4 + 10);
		}
		else
		{
			_progress.Size = new Vector2(256-8*8, 28);
			_progress.Position -= new Vector2(16-8, -32);
		}

		Sprite.Texture = Globals.Database.GetTexture(ItemType);
		GlobalPosition = Globals.TileMap.ToMap(GlobalPosition);
		if (BuildingSize.X % 2 == 0) Sprite.GlobalPosition += new Vector2(16, 0);
		if (BuildingSize.Y % 2 == 0) Sprite.GlobalPosition += new Vector2(0, 16);

		var positions = GetOccupiedPositions(GlobalPosition);
		positions.ForEach(pos => Globals.TileMap.AddEntity(pos, this, BuildingTileMap.Building));
		
		_audio.Play();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Globals.TileMap.RemoveEntity(this);
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
		_progress.Visible = false;
		FactoryGUI.Instance.BuildingRemoved();
		QueueFree();
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
	public virtual IRotatable.BuildingOrientation Orientation { get; set; }
	
	#region BuildingFactory
	/******************************************************************
	 * Building Factory                                               *
	 ******************************************************************/
	public static Building GetBuilding(string selectedItem, IRotatable.BuildingOrientation orientation)
	{
		return selectedItem switch
		{
			"Belt" => new Belt(orientation),
			"Underground Belt" => new UndergroundBelt(orientation),
			"Inserter" => new Inserter(orientation),
			"Long Inserter" => new Inserter(orientation, 2),
			"Mine" => new Mine(),
			"Stone Furnace" => new Furnace(),
			"Assembler" => new Assembler("Assembler", "None"),
			"Farm" => new Assembler("Farm", "farming"),
			"Armory" => new Assembler("Armory", "recruitment"),
			"House" => new Assembler("House", "population"),
			"Research Lab" => new ResearchLab(),
			"Barracks" => new Barracks(),
			"Stone Chest" => new StoneChest(),
			"Exporter" => new TradeDepot("Exporter"),
			"Importer" => new TradeDepot("Importer"),
			_ =>  throw new NotImplementedException()
		};
	}
	
	public static bool IsBuilding(string selectedItem)
	{
		return selectedItem switch
		{
			"Belt" => true,
			"Underground Belt" => true,
			"Inserter" => true,
			"Long Inserter" => true,
			"Mine" => true,
			"Stone Furnace" => true,
			"Assembler" => true,
			"Farm" => true,
			"Armory" => true,
			"House" => true,
			"Research Lab" => true,
			"Barracks" => true,
			"Stone Chest" => true,
			"Exporter" => true,
			"Importer" => true,
			_ => false
		};
	}
	#endregion

	#region Save/Load
	/******************************************************************
	 * Save/Load                                                      *
	 ******************************************************************/
	public virtual Godot.Collections.Dictionary<string, Variant> Save()
	{
		return new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", ItemType },
			{ "PosX", Position.X }, // Vector2 is not supported by JSON
			{ "PosY", Position.Y },
			{ "Orientation", Orientation.ToString() }
		};
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		Enum.TryParse(nodeData["Orientation"].ToString(), true, out IRotatable.BuildingOrientation orientation);
            
		var building = Building.GetBuilding(nodeData["ItemType"].ToString(), orientation);
		if (building is ICrafter crafter)
		{
			var recipeName = nodeData["Recipe"].ToString();
			var recipe = Globals.Database.GetRecipe(recipeName);
			crafter.SetRecipe(recipe);
		}
		
		building.GlobalPosition = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
		Globals.FactoryScene.AddChild(building);
	}
	#endregion
}
