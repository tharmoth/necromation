using System;
using System.Collections.Generic;
using Godot;

namespace Necromation.interactables.belts;

public partial class Belt : Building
{
    /**************************************************************************
     * Data                                                                   *
     **************************************************************************/
    protected float _secondsPerItem = .5333f;
    private readonly List<GroundItem> _itemsOnBelt = new();
    
    /**************************************************************************
     * Properties                                                             *
     **************************************************************************/
    public override string ItemType => "Belt";
    protected float Speed => BuildingTileMap.TileSize / _secondsPerItem;
    private Vector2I Output => MapPosition + TargetDirection;
    protected Vector2I MapPosition => Globals.TileMap.GlobalToMap(GlobalPosition);
    protected virtual Vector2I TargetDirection => Orientation switch {
        BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        BuildingOrientation.EastWest => new Vector2I(1, 0),
        BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public Belt(int degrees)
    {
        _orientation = degrees switch {
            0 => BuildingOrientation.NorthSouth,
            90 => BuildingOrientation.EastWest,
            180 => BuildingOrientation.SouthNorth,
            270 => BuildingOrientation.WestEast,
            _ => throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null)
        };
        
        RotationDegrees = degrees;
        SetNotifyTransform(true);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MovePlayer(delta);

        for (var i = 0; i < _itemsOnBelt.Count; i++)
        {
            var groundItem = _itemsOnBelt[i];
            var targetLocation = GetTargetLocation(i);
            
            // Slide the item along the belt until it reaches the bottom of the belt.
            if (!IsEqualApprox(groundItem.GlobalPosition, targetLocation, .5f))
            {
                groundItem.GlobalPosition += -targetLocation.DirectionTo(groundItem.GlobalPosition) * Speed * (float)delta;
                continue;
            }
            
            if (i != 0) continue;

            var nextBelt = GetNextBelt();
            if (nextBelt is not Belt belt) continue;
            if (!belt.CanAcceptItem()) continue;
            belt.AddItem(RemoveItem());
            i = 0;
        }
    }
    
    public GroundItem GetFirstItem()
    {
        return _itemsOnBelt.Count != 0 ? _itemsOnBelt[0] : null;
    }
    
    public bool CanAcceptItem()
    {
        return _itemsOnBelt.Count < 4;
    }
    
    public void AddItem(GroundItem item)
    {
        _itemsOnBelt.Add(item);
        
        GetTree().Root.AddChild(item);
        item.GlobalPosition = GetTargetLocation(3);
    }
    
    public GroundItem RemoveItem()
    {
        // Return the first item in the list and remove it from the list or else null.
        if (_itemsOnBelt.Count == 0) return null;
        var item = _itemsOnBelt[0];
        _itemsOnBelt.RemoveAt(0);
        GetTree().Root.RemoveChild(item);
        return item;
    }
    
    public void TransferAllTo(Inventory inventory)
    {
        _itemsOnBelt.ForEach(item => GetTree().Root.RemoveChild(item));
        _itemsOnBelt.ForEach(item => inventory.Insert(item.ItemType));
        _itemsOnBelt.Clear();
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    protected virtual BuildingTileMap.IEntity GetNextBelt()
    {
        return Globals.TileMap.GetEntities(Output, BuildingTileMap.LayerNames.Buildings);
    }
    
    protected Vector2 GetTargetLocation(int index)
    {
        return index switch
        {
            0 => GlobalPosition + TargetDirection * 16,
            1 => GlobalPosition + TargetDirection * 8,
            2 => GlobalPosition - TargetDirection * 0,
            3 => GlobalPosition - TargetDirection * 8,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };
    }

    private void MovePlayer(double delta)
    {
        if (Globals.TileMap.GlobalToMap(Globals.Player.GlobalPosition) != MapPosition) return;
        Globals.Player.GlobalPosition += -GetTargetLocation(0).DirectionTo(Globals.Player.GlobalPosition) * Speed * (float)delta;
    }
    
    private bool IsEqualApprox(Vector2 a, Vector2 b, float tolerance)
    {
        return Mathf.Abs(a.X - b.X) < tolerance && Mathf.Abs(a.Y - b.Y) < tolerance;
    }

    public override float GetProgressPercent()
    {
        return 0;
    }
}