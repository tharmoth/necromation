using System;
using System.Collections.Generic;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class UndergroundBelt : DoubleBelt
{
    /*
     * This class feels a bit hacky. There should be a more elegant way to handle the entrance/exit logic.
     * I'll have to think about it.
     */
    
    public override string ItemType => "Underground Belt";
    private const int Range = 4;
    public bool IsEntrance = true;
    protected override Vector2I TargetDirectionGlobal => IsEntrance ? Orientation switch {
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
        // Look for belts that might be the enterance to this one.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirectionGlobal * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt)
            {
                IsEntrance = !belt.IsEntrance;
                
                break;
            }
        }
        
        base._Ready();

        Globals.Player.RotateSelection();
        Globals.Player.RotateSelection();

        if (IsEntrance) return;
        var leftpos = LeftLine.Position;
        var rightpos = RightLine.Position;
        LeftLine.Position = rightpos;
        RightLine.Position = leftpos;
        LeftLine.RotationDegrees = 180;
        RightLine.RotationDegrees = 180;
    }

    protected override DoubleBelt GetOutputBelt()
    {
        // If this is not an enterance it just outputs to the next belt as usual.
        if (!IsEntrance) return base.GetOutputBelt();
        
        // If this is an entrance we need to try to find the next underground in range.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirectionGlobal * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt && belt.IsEntrance != IsEntrance) return belt;
        }
        return null;
    }
    
    /// <summary>
    /// Returns a dictionary of adjacent belts. If no belt is found in a direction, the corresponding dictionary value
    /// will be null.
    /// </summary>
    /// <returns>A dictionary with keys {"Forward", "Behind", "Right", "Left"} and values as the corresponding
    /// adjacent <see cref="DoubleBelt"/> instances or null if no belt is present in the direction.</returns>
    protected override Dictionary<string, DoubleBelt> GetAdjacent()
    {
        var rotatedRight = ((Vector2)TargetDirectionLocal).Rotated(Mathf.DegToRad(90)).Snapped(Vector2.One);
        var rotatedLeft = ((Vector2)TargetDirectionLocal).Rotated(Mathf.DegToRad(-90)).Snapped(Vector2.One);

        var beltForward = GetOutputBelt();
        DoubleBelt beltBehind = null;
        if (IsEntrance) beltBehind = GetBeltInDirection(-TargetDirectionLocal);
        else
        {
            for (var i = 1; i <= Range; i++)
            {
                var position = MapPosition - TargetDirectionGlobal * i;
                var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
                if (entity is UndergroundBelt belt && belt.IsEntrance != IsEntrance)
                {
                    GD.Print("AW shit");
                    beltBehind = belt;
                }
            }
        }

        var beltRight = GetBeltInDirection(rotatedRight);
        var beltLeft = GetBeltInDirection(rotatedLeft);

        return new Dictionary<string, DoubleBelt>
        {
            { "Forward", beltForward },
            { "Behind", beltBehind },
            { "Right", beltRight },
            { "Left", beltLeft }
        };
    }

    protected override void UpdateInputOutput(DoubleBelt belt, Dictionary<string, DoubleBelt> belts)
    {
        // There are 5 cases to consider
        // 1. If a behind belt inputs to this one link left and right
        // 2. If a left belt inputs to this one and there are no other inputs link left and right.
        // 3. if a left belt inputs to this one and there is another input link only left
        // 4. if a right belt inputs to this one and there are no other inputs link left and right.
        // 5. if a right belt inputs to this one and there is another input link only right
        var beltBehind = belts["Behind"];
        var beltRight = belts["Right"];
        var beltLeft = belts["Left"];
        
        var leftLine = belt?.LeftLine;
        var rightLine = belt?.RightLine;

        if (beltBehind != null)
        {
            beltBehind.LeftLine.OutputLine = leftLine;
            beltBehind.RightLine.OutputLine = rightLine;
        }
        if (beltLeft != null)
        {
            beltLeft.LeftLine.OutputLine = leftLine;
            beltLeft.RightLine.OutputLine = leftLine;
        }
        if (beltRight != null)
        {
            beltRight.LeftLine.OutputLine = rightLine;
            beltRight.RightLine.OutputLine = rightLine;
        }
    }
}