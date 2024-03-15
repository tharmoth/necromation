using System;
using System.Collections.Generic;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class TransportLine : ITransferTarget
{
    
    /*
     * Performance
     */
    private bool initalized = false;
    private Vector2 _cachePosition;

    private Vector2I _targetDirectionGlobal;
    public Vector2I TargetDirectionGlobal
    {
        get { return _targetDirectionGlobal; }
        set
        {
            _targetDirectionGlobal = value;
            _targetLocations.Clear();
            _targetLocations.Add(TargetDirectionGlobal * 16);
            _targetLocations.Add(TargetDirectionGlobal * 8);
            _targetLocations.Add(TargetDirectionGlobal * 0);
            _targetLocations.Add(TargetDirectionGlobal * -8);
            _targetLocations.Add(TargetDirectionGlobal * -16);
        }
    }

    private List<Vector2> _targetLocations = new();

    public float _secondsPerItem = .5333f;
    // public float _secondsPerItem = 1.0f;
    private const int MaxItems = 5;
    
    /**************************************************************************
     * Data                                                                   *
     **************************************************************************/
    
    private readonly List<GroundItem> _itemsOnBelt = new();
    private readonly TransportInventory _inventory = new();

    /**************************************************************************
     * Properties                                                             *
     **************************************************************************/
    public TransportLine OutputLine;
    protected float Speed => FactoryTileMap.TileSize / _secondsPerItem;
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public TransportLine()
    {
        _inventory.Listeners.Add(UpdateSprites);
        TargetDirectionGlobal = TargetDirectionGlobal;
    }

    public void Init(Vector2 globalPosition)
    {
        _cachePosition = globalPosition;
        for (int i = 0; i < _itemsOnBelt.Count; i++)
        {
            _itemsOnBelt[i].GlobalPosition = _cachePosition + GetTargetLocation(i);
        }
    }

    public void Process(double delta)
    {
        for (var i = 0; i < Mathf.Min(_itemsOnBelt.Count, 4); i++)
        {
            var groundItem = _itemsOnBelt[i];
            if (groundItem.CacheIndex != i)
            {
                var targetLocation = _cachePosition + GetTargetLocation(i);
                
                // Slide the item along the belt until it reaches the bottom of the belt.
                if (groundItem.GlobalPosition.DistanceTo(targetLocation) > Speed * (float)delta)
                {
                    var distance = -targetLocation.DirectionTo(groundItem.GlobalPosition) * Speed * (float)delta;
                    groundItem.GlobalPosition += distance;
                    continue;
                }
                else
                {
                    groundItem.CacheIndex = i;
                }
            }

            if (i != 0) continue;
            
            if (OutputLine == null) continue;
            if (!OutputLine.CanAcceptItems(groundItem.ItemType)) continue;
            BeltTransfer(OutputLine);
            i = 0;
        }
        
        initalized = true;
    }

    private void GetNextItem()
    {
        
    }

    public int GetItemCount(string item = null)
    {
        if (item == null) return _inventory.CountItems();
        return _inventory.CountItem(item);
    }

    /**************************************************************************
     * Protected Methods                                                      *
     **************************************************************************/
    protected Vector2 GetTargetLocation(int index)
    {
        return _targetLocations[index];
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
        item.CacheIndex = -1;
        Inventory.TransferItem(this, to, item.ItemType);
    }

    private void UpdateSprites()
    {
        while (_inventory.CountItems() != _itemsOnBelt.Count)
        {
            if (_inventory.CountItems() < _itemsOnBelt.Count)
            {
                RemoveItem();
            }
            else
            {
                var newItem = new GroundItem(GetMissingItem());
                AddItem(newItem);
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
        if (IsEqualApprox(item.GlobalPosition, Vector2.Zero, 1))
        {
            item.GlobalPosition = _cachePosition + GetTargetLocation(_itemsOnBelt.Count - 1);
        }
        else
        {
            int i = 0;
            i++;
        }
    }
    
    private GroundItem RemoveItem()
    {
        // Return the first item in the list and remove it from the list or else null.
        if (_itemsOnBelt.Count == 0) return null;
        var item = _itemsOnBelt[0];
        _itemsOnBelt.RemoveAt(0);
        item.Free();
        return item;
    }

    public static bool IsEqualApprox(Vector2 a, Vector2 b, float tolerance = .001f) => Mathf.Abs(a.X - b.X) < tolerance && Mathf.Abs(a.Y - b.Y) < tolerance;
    public static bool IsEqualApprox(float a, float b, float tolerance = .001f) => Mathf.Abs(a - b) < tolerance;

    #region ITransferTarget Implementation/
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item,  int count = 1) => _inventory.CanAcceptItems(item, count);
    public bool CanAcceptItemsInserter(string item,  int count = 1) => _inventory.CanAcceptItemsInserter(item, count);
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => _inventory.GetInventories();
    private class TransportInventory : Inventory
    {
        public override int GetMaxTransferAmount(string itemType)
        {
            return Mathf.Max(0,MaxItems - CountItems());
        }
    }
    #endregion

    // This is needed for saveload where items are not placed in an inventory so the groundItems to get removed.
    public void _ExitTree()
    {
        foreach (var groundItem in _itemsOnBelt)
        {
            groundItem.Free();
        }
    }
}