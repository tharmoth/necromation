using System;
using Godot;

namespace Necromation.interactables.belts;

public partial class UndergroundBelt : Belt
{
    public override string ItemType => "Underground Belt";
    private const int Range = 4;
    public bool IsEntrance = true;
    protected override Vector2I TargetDirection => IsEntrance ? Orientation switch {
        BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        BuildingOrientation.EastWest => new Vector2I(1, 0),
        BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    } :
        Orientation switch {
        BuildingOrientation.NorthSouth => new Vector2I(0, 1),
        BuildingOrientation.EastWest => new Vector2I(-1, 0),
        BuildingOrientation.SouthNorth => new Vector2I(0, -1),
        BuildingOrientation.WestEast => new Vector2I(1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public UndergroundBelt(int degrees) : base(degrees)
    {
    }

    public override void _Ready()
    {
        base._Ready();
        GD.Print("I'm at " + MapPosition);
        // Look for belts that might be the enterance to this one.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirection * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt)
            {
                GD.Print("Found Enterance at " + position);
                IsEntrance = !belt.IsEntrance;
                break;
            }
            else
            {
                GD.Print("No Enterance at " + position);

            
            }
        }
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