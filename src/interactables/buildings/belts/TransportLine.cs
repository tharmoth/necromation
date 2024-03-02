using System;
using System.Collections.Generic;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class TransportLine : Node2D, ITransferTarget
{
    private bool initalized = false;
    
    /**************************************************************************
     * Data                                                                   *
     **************************************************************************/
    protected float _secondsPerItem = .5333f;
    private readonly List<GroundItem> _itemsOnBelt = new();
    private readonly Inventory _inventory = new();

    /**************************************************************************
     * Properties                                                             *
     **************************************************************************/
    public TransportLine OutputLine;
    protected float Speed => BuildingTileMap.TileSize / _secondsPerItem;
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public TransportLine()
    {
        _inventory.Listeners.Add(UpdateSprites);
    }

    public override void _Ready()
    {
        base._Ready();
        int i = 0;
        _itemsOnBelt.ForEach(item =>
        {
           item.GlobalPosition = GlobalPosition + GetTargetLocation(4);
        });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        for (var i = 0; i < _itemsOnBelt.Count; i++)
        {
            var groundItem = _itemsOnBelt[i];
            var targetLocation = ToGlobal(Position + GetTargetLocation(i));
            
            // Slide the item along the belt until it reaches the bottom of the belt.
            if (!IsEqualApprox(groundItem.GlobalPosition, targetLocation, 1.0f))
            {
                groundItem.GlobalPosition += -targetLocation.DirectionTo(groundItem.GlobalPosition) * Speed * (float)delta;
                groundItem.CachePosition = GlobalPosition;
                continue;
            }
            
            if (i != 0) continue;
            
            if (OutputLine == null) continue;
            if (!OutputLine.CanAcceptItems(groundItem.ItemType)) continue;
            BeltTransfer(OutputLine);
            i = 0;
        }
        
        initalized = true;
    }

    public int GetItemCount(string item = null)
    {
        if (item == null) return _inventory.CountAllItems();
        return _inventory.CountItem(item);
    }

    /**************************************************************************
     * Protected Methods                                                      *
     **************************************************************************/
    protected Vector2 GetTargetLocation(int index)
    {
        return index switch
        {
            0 => new Vector2I(0, -1) * 16,
            1 => new Vector2I(0, -1) * 8,
            2 => new Vector2I(0, -1) * 0,
            3 => new Vector2I(0, -1) * -8,
            4 => new Vector2I(0, -1) * -16,
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
    private void BeltTransfer(TransportLine to)
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
        var pos = item.GlobalPosition;
        if (item.GetParent() == null) Globals.FactoryScene.AddChild(item);
        if (IsEqualApprox(item.Position, Vector2.Zero, 1))
        {
            item.GlobalPosition = GlobalPosition + GetTargetLocation(4);
            item.CachePosition = GlobalPosition + GetTargetLocation(4);
        }
        else
        {
            item.GlobalPosition = pos;
            item.CachePosition = pos;
        }
        item.GlobalRotation = 0;
    }
    
    private GroundItem RemoveItem()
    {
        // Return the first item in the list and remove it from the list or else null.
        if (_itemsOnBelt.Count == 0) return null;
        var item = _itemsOnBelt[0];
        _itemsOnBelt.RemoveAt(0);
        Globals.FactoryScene.RemoveChild(item);
        return item;
    }

    public static bool IsEqualApprox(Vector2 a, Vector2 b, float tolerance = .001f) => Mathf.Abs(a.X - b.X) < tolerance && Mathf.Abs(a.Y - b.Y) < tolerance;
    public static bool IsEqualApprox(float a, float b, float tolerance = .001f) => Mathf.Abs(a - b) < tolerance;
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