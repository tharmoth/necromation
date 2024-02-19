﻿using System;
using System.Collections.Generic;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class TransportLine : Node2D, ITransferTarget
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
    public TransportLine OutputLine;
    protected float Speed => BuildingTileMap.TileSize / _secondsPerItem;
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public TransportLine()
    {
        _inventory.Listeners.Add(UpdateSprites);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        for (var i = 0; i < _itemsOnBelt.Count; i++)
        {
            var groundItem = _itemsOnBelt[i];
            var targetLocation = GetTargetLocation(i);
            
            // Slide the item along the belt until it reaches the bottom of the belt.
            if (!IsEqualApprox(groundItem.Position, targetLocation, .5f))
            {
                groundItem.Position += -targetLocation.DirectionTo(groundItem.Position) * Speed * (float)delta;
                continue;
            }
            
            if (i != 0) continue;
            
            if (OutputLine == null) continue;
            if (!OutputLine.CanAcceptItems(groundItem.ItemType)) continue;
            BeltTransfer(OutputLine);
            i = 0;
        }
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
        if (item.GetParent() != null) item.GetParent().RemoveChild(item);
        if (item.GetParent() != this) AddChild(item);
        item.Position = GetTargetLocation(4);
    }
    
    private GroundItem RemoveItem()
    {
        // Return the first item in the list and remove it from the list or else null.
        if (_itemsOnBelt.Count == 0) return null;
        var item = _itemsOnBelt[0];
        _itemsOnBelt.RemoveAt(0);
        RemoveChild(item);
        return item;
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