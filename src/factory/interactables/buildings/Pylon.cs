
using System.Collections.Generic;
using Godot;
using Necromation;
using Necromation.components;
using Necromation.interfaces;

public class Pylon : Building , ITransferTarget, IInteractable
{
    /**************************************************************************
     * Building Implementation                                                *
     **************************************************************************/
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Pylon";
    
    private readonly Inventory _inventory = new();
    private readonly FuelComponent _fuelComponent;

    public Pylon() : base()
    {
        _fuelComponent = new FuelComponent() {InputInventory = _inventory};
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _fuelComponent._Process(delta);
    }

    public bool DrawPower(int i)
    {
        return _fuelComponent.DrawPower();;
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public void Interact(Inventory playerInventory)
    {
        ContainerGui.Display(playerInventory, _inventory, ItemType);
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public virtual bool CanAcceptItems(string item,  int count = 1) => _inventory.CanAcceptItems(item, count);
    public virtual bool CanAcceptItemsInserter(string item,  int count = 1) => _inventory.CanAcceptItemsInserter(item, count);
    public virtual void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    public int GetMaxTransferAmount(string itemType) => _inventory.GetMaxTransferAmount(itemType);
    private class ChestInventory : Inventory
    {
        public override int GetMaxTransferAmount(string itemType)
        {
            return 200 - CountItems();
        }
    }
    #endregion
}