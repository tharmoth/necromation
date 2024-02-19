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
    private bool _isEntrance = true;
    protected override Vector2I TargetDirectionGlobal => _isEntrance ? Orientation switch {
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
                _isEntrance = !belt._isEntrance;
                
                break;
            }
        }
        
        base._Ready();

        Globals.Player.RotateSelection();
        Globals.Player.RotateSelection();

        if (_isEntrance) return;
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
        if (!_isEntrance) return base.GetOutputBelt();
        
        // If this is an entrance we need to try to find the next underground in range.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirectionGlobal * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt && belt._isEntrance != _isEntrance) return belt;
        }
        return null;
    }

    protected override DoubleBelt GetBehindBelt()
    {
        if (_isEntrance) return base.GetBehindBelt();

        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition - TargetDirectionGlobal * i;
            var entity = Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Buildings);
            if (entity is UndergroundBelt belt && belt._isEntrance != _isEntrance) return belt;
        }

        return null;
    }
    
    protected override void UpdateInputOutput(DoubleBelt belt, Dictionary<string, DoubleBelt> belts)
    {
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