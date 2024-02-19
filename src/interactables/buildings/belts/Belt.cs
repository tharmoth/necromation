using System;
using System.Collections.Generic;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class Belt : Building, ITransferTarget, IRotatable
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
    private IRotatable.BuildingOrientation _orientation;

    public IRotatable.BuildingOrientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            RotationDegrees = IRotatable.GetDegreesFromOrientation(value);
        }
    }

    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Belt";
    protected float Speed => BuildingTileMap.TileSize / _secondsPerItem;
    private Vector2I Output => MapPosition + TargetDirection;
    protected Vector2I MapPosition => Globals.TileMap.GlobalToMap(GlobalPosition);
    protected virtual Vector2I TargetDirection => Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public Belt()
    {
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
            if (!belt.CanAcceptItems(groundItem.ItemType)) continue;
            BeltTransfer(belt);
            i = 0;
        }
    }

    /**************************************************************************
     * Protected Methods                                                      *
     **************************************************************************/
    protected virtual BuildingTileMap.IEntity GetNextBelt()
    {
        return Globals.TileMap.GetEntities(Output, BuildingTileMap.Building);
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
        Inventory.TransferItem(this, to, item.ItemType);
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
    
    public static bool IsEqualApprox(Vector2 a, Vector2 b, float tolerance) => Mathf.Abs(a.X - b.X) < tolerance && Mathf.Abs(a.Y - b.Y) < tolerance;

    #region ITransferTarget Implementation/
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item,  int count = 1) => _inventory.CountAllItems() + count < 5;
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => _inventory.GetInventories();
    #endregion
}