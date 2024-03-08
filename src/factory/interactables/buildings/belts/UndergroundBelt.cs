using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class UndergroundBelt : Belt
{
    /*
     * This class feels a bit hacky. There should be a more elegant way to handle the entrance/exit logic.
     * I'll have to think about it.
     */
    
    public override string ItemType => "Underground Belt";
    private const int Range = 4;
    private bool _isEntrance = true;
    // protected override Vector2I TargetDirectionGlobal => _isEntrance ? Orientation switch {
    //     IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
    //     IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
    //     IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
    //     IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
    //     _ => throw new ArgumentOutOfRangeException()
    // } :
    //     Orientation switch {
    //     IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, 1),
    //     IRotatable.BuildingOrientation.EastWest => new Vector2I(-1, 0),
    //     IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, -1),
    //     IRotatable.BuildingOrientation.WestEast => new Vector2I(1, 0),
    //     _ => throw new ArgumentOutOfRangeException()
    // };

    public override void _Ready()
    {
        GD.Print("Placed Underground Belt at " + MapPosition + " with orientation " + Orientation);
        // Look for belts that might be the enterance to this one. do this before belt._Ready() is called and rotates things.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition - TargetDirectionGlobal * i;
            var entity = Globals.FactoryScene.TileMap.GetEntity(position, FactoryTileMap.Building);
            if (entity is Belt belty)
            {
                GD.Print("Checking " + entity + " at " + position + " with orientation " + belty.Orientation);
            }
            else
            {
                GD.Print("No belt at " + position);
            }
            
            if (entity is UndergroundBelt belt && belt.Orientation  == Orientation)
            {
                _isEntrance = !belt._isEntrance;
                GD.Print("Found belt at " + position + " with orientation " + belt.Orientation + " and isEntrance " + belt._isEntrance);
                break;
            }
        }
        
        base._Ready();
        
        // TODO: Resetting the texture to the correct one. Belt does animation things. This is bad and should be refactored.
        Sprite.Texture = Database.Instance.GetTexture(ItemType);
        Sprite.Hframes = 1;

        // Globals.Player.RotateSelection();
        // Globals.Player.RotateSelection();

        if (_isEntrance) return;
        Sprite.GlobalRotation += Mathf.Pi;
        // We want to flip the lanes for the exit.
        LeftLine.Init(GlobalPosition + new Vector2(8, 0).Rotated(Sprite.GlobalRotation));
        RightLine.Init(GlobalPosition + new Vector2(-8, 0).Rotated(Sprite.GlobalRotation));
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        return GetOccupiedPositions(position).All(Globals.FactoryScene.TileMap.IsBuildable);
    }

    protected override Belt GetOutputBelt()
    {
        // If this is an exit it just outputs to the next belt as usual.
        if (!_isEntrance) return base.GetOutputBelt();
        
        // If this is an entrance we need to try to find the next underground in range.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition + TargetDirectionGlobal * i;
            var entity = Globals.FactoryScene.TileMap.GetEntity(position, FactoryTileMap.Building);
            if (entity is UndergroundBelt belt && belt._isEntrance != _isEntrance) return belt;
        }
        return null;
    }

    protected override Belt GetBehindBelt()
    {
        // If this is an enterance the behind belt is the previous belt as usual.
        if (_isEntrance) return base.GetBehindBelt();

        // If this is an exit we need to try to find the previous underground in range.
        for (var i = 1; i <= Range; i++)
        {
            var position = MapPosition - TargetDirectionGlobal * i;
            var entity = Globals.FactoryScene.TileMap.GetEntity(position, FactoryTileMap.Building);
            if (entity is UndergroundBelt belt && belt._isEntrance != _isEntrance)
            {
                LeftLine._secondsPerItem = 0.133325f;
                RightLine._secondsPerItem = 0.133325f;
                return belt;
            }
        }

        return null;
    }
    
    protected override void UpdateInputOutput(Belt belt, Dictionary<string, Belt> belts)
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

    public UndergroundBelt(IRotatable.BuildingOrientation orientation) : base(orientation)
    {
    }
}