using Godot;
using System;

public abstract partial class Building : Node2D, BuildingTileMap.IBuilding, BuildingTileMap.IEntity, ProgressTracker.IProgress
{
	public enum BuildingOrientation
	{
		NorthSouth,
		EastWest,
		SouthNorth,
		WestEast
	}

	protected BuildingOrientation _orientation;
	public BuildingOrientation Orientation => _orientation;
	
	protected Sprite2D _sprite;

	protected Building()
	{
		_sprite = new Sprite2D();
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
		_sprite.Texture = GD.Load<Texture2D>($"res://res/sprites/{ItemType}.png");
	}

	public abstract string ItemType { get; }
	public abstract float GetProgressPercent();
	
	public static BuildingOrientation GetOppositeOrientation(BuildingOrientation orientation)
	{
		return orientation switch
		{
			BuildingOrientation.NorthSouth => BuildingOrientation.SouthNorth,
			BuildingOrientation.EastWest => BuildingOrientation.WestEast,
			BuildingOrientation.SouthNorth => BuildingOrientation.NorthSouth,
			BuildingOrientation.WestEast => BuildingOrientation.EastWest,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
