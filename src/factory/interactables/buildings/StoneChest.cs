using System.Collections.Generic;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class StoneChest : Building, ITransferTarget, IInteractable
{
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Stone Chest";
    protected Inventory _inventory = new ChestInventory();

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public void Interact(Inventory playerInventory)
    {
        ContainerGui.Display(playerInventory, _inventory, ItemType);;
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