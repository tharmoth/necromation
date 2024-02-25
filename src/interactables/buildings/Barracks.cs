﻿using System.Collections.Generic;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class Barracks : Building, IInteractable, ITransferTarget
{
    private Inventory _inventory = new();
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Barracks";
    
    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public void Interact(Inventory playerInventory)
    {
        GUI.Instance.Display(playerInventory, _inventory, ItemType);
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1)
    {
        return item == "Warrior" || item == "Archer" && GetInventories().TrueForAll(inventory => inventory.CountItem(item) < 10);
    }
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
}