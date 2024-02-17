using System;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class UndergroundBelt : Belt
{
    public override string ItemType => "Underground Belt";
    private const int Range = 4;
    public bool IsEntrance = true;
    protected override Vector2I TargetDirection => IsEntrance ? Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    } :
        Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(-1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };

    public override void _Ready()
    {
        base._Ready();
        // Look for belts that might be the enterance to this one.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirection * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt)
            {
                IsEntrance = !belt.IsEntrance;
                break;
            }
        }
        
        Globals.Player.RotateSelection();
        Globals.Player.RotateSelection();
    }

    protected override BuildingTileMap.IEntity GetNextBelt()
    {
        if (!IsEntrance) return base.GetNextBelt();
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirection * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt && belt.IsEntrance != IsEntrance) return belt;
        }
        return null;
    }
}