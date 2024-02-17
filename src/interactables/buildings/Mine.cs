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

        if (GetProgressPercent() < 1.0f) return;
        _time = 0;

        var resource = Globals.TileMap.GetEntityPositions(this)
            .Select(position => Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Resources))
            .First(resource => resource is Collectable);
        
        if (resource is not Collectable collectable) return;
        _inventory.Insert(collectable.ItemType);
    }
    
    public override float GetProgressPercent()
    {
        return _time / _miningSpeed;
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
    public void Interact()
    {
         GUI.Instance.ContainerGui.Display(Globals.PlayerInventory, _inventory, ItemType);
    }
    #endregion
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1, Vector2 position = default)
    {
        return false;
    }
    public void Insert(string item, int count = 1, Vector2 position = default)
    {
        return;
    }

    public bool Remove(string item, int count = 1)
    {
        return _inventory.Remove(item, count);
    }

    public string GetFirstItem()
    {
        return _inventory.GetFirstItem();
    }
    
    public List<Inventory> GetInventories()
    {
        return new List<Inventory> { _inventory };
    }
    #endregion
}