using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class Mine : Building, IInteractable, ITransferTarget
{
    public override Vector2I BuildingSize => Vector2I.One * 2;
    private Inventory _inventory = new();
    private float _time;
    private float _miningSpeed = 1.0f;

    public override string ItemType => "Mine";

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (MaxOutputItemsReached())
        {
            _time = 0;
            return;
        }

        _time += (float)delta;

        if (GetProgressPercent() < 2.0f) return;
        _time = 0;

        var resource = Globals.TileMap.GetEntityPositions(this)
            .Select(position => Globals.TileMap.GetEntity(position, BuildingTileMap.Resource))
            .First(resource => resource is Collectable);
        
        if (resource is not Collectable collectable) return;
        _inventory.Insert(collectable.ItemType);
    }
    
    public override float GetProgressPercent()
    {
        return _time / _miningSpeed;
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) && GetOccupiedPositions(position).Any(Globals.TileMap.IsResource);
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private bool MaxOutputItemsReached()
    {
        return _inventory.CountAllItems() >= 200;
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
        FactoryGUI.Instance.Display(playerInventory, _inventory, ItemType);
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => false;
    public void Insert(string item, int count = 1) { }
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
}