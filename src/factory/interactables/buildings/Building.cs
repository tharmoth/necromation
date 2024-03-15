using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.factory;
using Necromation.gui;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;
using Necromation.sk;

public abstract partial class Building : FactoryTileMap.IEntity, ProgressTracker.IProgress
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly ShaderMaterial GreyScale = GD.Load<ShaderMaterial>("res://src/factory/shaders/greyscale.tres");
	// private static readonly PackedScene ParticleScene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/place_particles.tscn");
	private static readonly PackedScene ParticleScene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/dirt.tscn");
	private static readonly AudioStream PlaceSound = GD.Load<AudioStream>("res://res/sfx/zapsplat_foley_boots_wellington_rubber_pair_set_down_grass_001_105602.mp3");
	private static readonly AudioStream GrindSound = GD.Load<AudioStream>("res://res/sfx/zapsplat_transport_bicycle_ride_gravel_onboard_pov_10530.mp3");

	/**************************************************************************
	 * Utility Property                                                       *
	 **************************************************************************/
	public Vector2I MapPosition => Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
	public Vector2 GlobalPosition
	{
		get => _globalPosition;
		set => _globalPosition = value;
	}
	public Vector2 GetSpriteOffset() => BuildingSize.X % 2 == 0 ? new Vector2(16, 16) : new Vector2(0, 0);
	public string Id => _id;
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private Vector2 _globalPosition;
	protected bool IsOnScreen = true;
	// Unique identifier for communicating data linked to the Map/Battle on save/load
	private string _id = Guid.NewGuid().ToString();
	
	/**************************************************************************
	 * Visuals Variables 													  *
	 **************************************************************************/
	public readonly Node2D BaseNode = new ();
	public readonly Sprite2D Sprite = new();
	
	protected readonly Sprite2D GhostSprite = new();
	protected readonly ColorRect ClipRect = new();
	
	private readonly AudioStreamPlayer2D _audio = new();
	private readonly GpuParticles2D _particles;
	private readonly VisibleOnScreenNotifier2D _notifier = new();
	
	
	/// <summary>
	/// Is true when the build animation is complete. Can be used to delay the building's functionality until the animation is complete.
	/// </summary>
	protected bool BuildComplete = false;

	protected Building()
	{
		_notifier.ScreenEntered += () => IsOnScreen = true;
		_notifier.ScreenExited += () => IsOnScreen = false;
		
		_audio.Stream = GrindSound;
		
		_particles = ParticleScene.Instantiate<GpuParticles2D>();
		_particles.OneShot = true;
		_particles.ZAsRelative = true;
		_particles.ZIndex = -1;
		
		Sprite.AddChild(_audio);
		Sprite.AddChild(_notifier);
		
		ClipRect.AddChild(Sprite);
		ClipRect.ClipChildren = CanvasItem.ClipChildrenMode.Only;
		ClipRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		
		GhostSprite.Modulate = new Color(1, 1, 1, .25f);
		GhostSprite.Material = GreyScale;
		GhostSprite.ZIndex = 100;

		BaseNode.AddChild(_particles);
		BaseNode.AddChild(GhostSprite);
		BaseNode.AddChild(ClipRect);
	}

	public virtual void _Process(double delta)
	{
		// if (Sprite.Visible)
		// {
		// 	if (!IsOnScreen)
		// 	{
		// 		Sprite.Visible = false;
		// 	}
		// } else if (!Sprite.Visible)
		// {
		// 	if (Notifier.IsOnScreen())
		// 	{
		// 		Sprite.Visible = true;
		// 	}
		// }
	}

	public virtual void _Ready()
	{
		IsOnScreen = _notifier.IsOnScreen();
		GlobalPosition = Globals.FactoryScene.TileMap.ToMap(GlobalPosition);
		BaseNode.GlobalPosition = GlobalPosition;

		var positions = GetOccupiedPositions(GlobalPosition);
		positions.ForEach(pos => Globals.FactoryScene.TileMap.AddEntity(pos, this, FactoryTileMap.Building));
		
		Sprite.Texture = Database.Instance.GetTexture(ItemType);
		
		var spriteSize = Utils.Max(Sprite.Texture.GetSize());
		ClipRect.Position = GetSpriteOffset() - new Vector2(spriteSize, spriteSize) / 2;
		ClipRect.CustomMinimumSize = new Vector2(spriteSize, spriteSize);
		ClipRect.CustomMinimumSize += Vector2.One * 8;
		
		GhostSprite.Texture = Database.Instance.GetTexture(ItemType);
		GhostSprite.GlobalPosition = GlobalPosition + GetSpriteOffset();

		var particleMaterial = (ParticleProcessMaterial) _particles.ProcessMaterial;
		particleMaterial.EmissionBoxExtents = new Vector3(16 * BuildingSize.X, 16 * BuildingSize.Y, 0);
		_particles.GlobalPosition = GlobalPosition + GetSpriteOffset();

		PlayBuildAnimation();
	}

	public virtual void _ExitTree()
	{
		Globals.FactoryScene.TileMap.RemoveEntity(this);
	}

	private void PlayBuildAnimation()
	{
		var clipTarget = ClipRect.Position.Y;
		ClipRect.Position += Vector2.Up * Utils.Max(Sprite.Texture.GetSize());
		
		var spriteTarget = Sprite.Texture.GetSize() / 2;
		Sprite.Position = spriteTarget;
		Sprite.Position -= Vector2.Up * Utils.Max(Sprite.Texture.GetSize()) * 2;
		Sprite.Position -= GD.Randf() > .5 ? Vector2.Right * 5 : Vector2.Left * 5;
		
		Animate(spriteTarget, clipTarget, () =>
		{
			BuildComplete = true;
			GhostSprite.Visible = false;
		});
	}

	private void PlayRemoveAnimation()
	{
		var clipTarget = ClipRect.Position.Y - Utils.Max(Sprite.Texture.GetSize());
		
		var spriteTarget = Sprite.Position;
		spriteTarget -= Vector2.Up * Utils.Max(Sprite.Texture.GetSize()) * 2;
		spriteTarget -= GD.Randf() > .5 ? Vector2.Right * 5 : Vector2.Left * 5;
		
		Animate(spriteTarget, clipTarget, BaseNode.QueueFree);
	}

	private void Animate(Vector2 spriteTarget, float clipTarget, Action onComplete)
	{
		_particles.Emitting = true;

		_audio.CallDeferred("play", (float)GD.RandRange(0.0f, 20.0f));
		_audio.VolumeDb = -20;
		_audio.PitchScale = .25f;

		var tweenTime = .5 * BuildingSize.Y;
		
		var yTween = Globals.Tree.CreateTween();
		yTween.TweenProperty(Sprite, "position:y", spriteTarget.Y, tweenTime);
		var xTween = Globals.Tree.CreateTween();
		xTween.SetEase(Tween.EaseType.InOut);
		
		var jiggleCount = 5;
		for (var i = 0; i < jiggleCount - 1; i++)
		{
			xTween.TweenProperty(Sprite, "position:x", 
				spriteTarget.X + GD.RandRange(-3, 3), tweenTime / jiggleCount);
		}
		
		xTween.TweenProperty(Sprite, "position:x", spriteTarget.X, tweenTime / jiggleCount);
		xTween.TweenCallback(Callable.From(() => _audio.Stop()));
		xTween.TweenCallback(Callable.From(onComplete));

		var tweenTest = Globals.Tree.CreateTween();
		tweenTest.TweenProperty(ClipRect, "position:y", clipTarget, tweenTime);
		tweenTest.TweenCallback(Callable.From(() =>
		{
			GD.Print("Tween Complete");
			GD.Print(ClipRect.Position);
		}));
	}
	

	/******************************************************************
	 * Public Methods                                                 *
	 ******************************************************************/
	public virtual bool CanPlaceAt(Vector2 position)
	{
		return GetOccupiedPositions(position).All(Globals.FactoryScene.TileMap.IsBuildable);
	}
	
	public List<Vector2I> GetOccupiedPositions(Vector2 position)
	{
		position = Globals.FactoryScene.TileMap.ToMap(position);
		if (BuildingSize.X % 2 == 0) position += new Vector2(16, 0);
		if (BuildingSize.Y % 2 == 0) position += new Vector2(0, 16);
		var topLeft = Globals.FactoryScene.TileMap.GlobalToMap(position) - BuildingSize / 2;

		var positions = (from x in Enumerable.Range(0, BuildingSize.X)
			from y in Enumerable.Range(0, BuildingSize.Y)
			select topLeft + new Vector2I(x, y)).ToList();
		return positions;
	}

	public virtual void Remove(Inventory to)
	{
		Globals.FactoryScene.TileMap.RemoveEntity(this);
		Globals.BuildingManager.RemoveBuilding(this);
		
		if (to != null)
		{
			TransferInventories(to);
			to.Insert(ItemType);
			SKFloatingLabel.Show("+1 " + ItemType + " (" + to.CountItem(ItemType) + ")", Sprite.GlobalPosition + new Vector2(0, 0));
		}

		PlayRemoveAnimation();
	}

	protected void TransferInventories(Inventory to)
	{
		int labelOffset = 1;
		if (this is not ITransferTarget inputTarget) return;
		foreach (var from in inputTarget.GetInventories())
		{
			foreach (var item in from.GetItems())
			{
				var count = from.CountItem(item);
				Inventory.TransferItem(from, to, item, count);
				var remaining = to.CountItem(item);
				SKFloatingLabel.Show("+" + count + " " + item + " (" + remaining + ")", Sprite.GlobalPosition + new Vector2(0, labelOffset++ * 20));
			}
		}
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
	public virtual void _PhysicsProcess(double delta) { }
	
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
			"Barracks" => new Barracks("recruitment"),
			"Stone Chest" => new StoneChest(),
			"Void Chest" => new VoidChest(),
			"Infinite Chest" => new InfiniteChest(),
			"Loader" => new Loader(orientation),
			_ => null
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
			"Void Chest" => true,
			"Infinite Chest" => true,
			"Loader" => true,
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
		var dict =  new Godot.Collections.Dictionary<string, Variant>()
		{
			{ "ItemType", ItemType },
			{ "Id", Id},
			{ "PosX", GlobalPosition.X }, // Vector2 is not supported by JSON
			{ "PosY", GlobalPosition.Y },
			{ "Orientation", Orientation.ToString() }
		};
		if (this is ICrafter crafter)
		{
			dict["Recipe"] = crafter.GetRecipe() != null ? crafter.GetRecipe().Name : "";
		}
		if (this is ITransferTarget transferTarget)
		{
			for (var i = 0; i < transferTarget.GetInventories().Count; i++)
			{
				dict["Inventory" + i] = transferTarget.GetInventories()[i].Save();
			}
		}
		return dict;
	}
	
	public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
	{
		Enum.TryParse(nodeData["Orientation"].ToString(), true, out IRotatable.BuildingOrientation orientation);
		          
		var building = Building.GetBuilding(nodeData["ItemType"].ToString(), orientation);
		if (building is ICrafter crafter)
		{
			var recipeName = nodeData["Recipe"].ToString();
			if (recipeName != "") 
			{
				var recipe = Database.Instance.GetRecipe(recipeName);
				crafter.SetRecipe(null, recipe);
			}
		}
		
		if (building is ITransferTarget transferTarget)
		{
			for (var i = 0; i < transferTarget.GetInventories().Count; i++)
			{
				transferTarget.GetInventories()[i].Load((Godot.Collections.Dictionary)nodeData["Inventory" + i]);
			}
		}
		
		building.GlobalPosition = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
		building._audio.Stream = null;
		building._id = nodeData["Id"].ToString();
		Globals.BuildingManager.AddBuilding(building, building.GlobalPosition);
	}
	#endregion
}
