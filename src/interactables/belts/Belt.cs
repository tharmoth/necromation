using System;
using System.Collections.Generic;
using Godot;

namespace Necromation.interactables.belts;

public partial class Belt : Building, Inserter.ITransferTarget
{
    /**************************************************************************
     * Data                                                                   *
     **************************************************************************/
    protected float _secondsPerItem = .5333f;
    private readonly List<GroundItem> _itemsOnBelt = new();
    private readonly Inventory _inventory = new();
    
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
        
        _inventory.Listeners.Add(UpdateSprites);
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
            if (!belt.CanAcceptItem(groundItem.ItemType)) continue;
            BeltTransfer(belt);
            i = 0;
        }
    }
    
    public override float GetProgressPercent()
    {
        return 0;
    }

    public Inventory GetInputInventory()
    {
        return _inventory;
    }

    public Inventory GetOutputInventory()
    {
        return _inventory;
    }
    
    /**************************************************************************
     * Protected Methods                                                      *
     **************************************************************************/

    public bool CanAcceptItem(string item)
    {
        return _itemsOnBelt.Count < 4;
    }
    
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
            4 => GlobalPosition - TargetDirection * 16,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
        /*
     * Transfer an item from this belt to the next belt bypassing the inventory.
     * This is done so that the ground item can be moved to the next belt without
     * removing it from the scene tree. Also avoids z level issues.
     */
    private void BeltTransfer(Belt to)
    {
        var item = _itemsOnBelt[0];
        _itemsOnBelt.RemoveAt(0);
        to.AddItem(item);
        Inventory.TransferItem(_inventory, to.GetInputInventory(), item.ItemType);
    }

    private void UpdateSprites()
    {
        while (_inventory.CountAllItems() != _itemsOnBelt.Count)
        {
            if (_inventory.CountAllItems() < _itemsOnBelt.Count)
            {
                RemoveItem();
            }
            else
            {
                AddItem(new GroundItem(GetMissingItem()));
            }
        }
    }

    private string GetMissingItem()
    {
        // Build an inventory dictionary of the items on the belt and compare it to the inventory to determine what to add.
        var beltItems = new Dictionary<string, int>();
        foreach (var item in _itemsOnBelt)
        {
            beltItems.TryGetValue(item.ItemType, out var count);
            beltItems[item.ItemType] = count + 1;
        }
                
        // Add the first item that is in the inventory but not on the belt.
        foreach (var (item, count) in _inventory.Items)
        {
            if (beltItems.TryGetValue(item, out var beltCount) && beltCount >= count) continue;
            return item;
        }

        return null;
    }
    
    private void AddItem(GroundItem item)
    {
        _itemsOnBelt.Add(item);
        if (item.GetParent() == null) GetTree().Root.AddChild(item);
        item.GlobalPosition = GetTargetLocation(4);
    }
    
    private GroundItem RemoveItem()
    {
        // Return the first item in the list and remove it from the list or else null.
        if (_itemsOnBelt.Count == 0) return null;
        var item = _itemsOnBelt[0];
        _itemsOnBelt.RemoveAt(0);
        GetTree().Root.RemoveChild(item);
        return item;
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
}