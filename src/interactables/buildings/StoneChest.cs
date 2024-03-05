﻿using System.Collections.Generic;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class StoneChest : Building, ITransferTarget, IInteractable
{
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Stone Chest";
    private Inventory _inventory = new();

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public void Interact(Inventory playerInventory)
    {
        Globals.FactoryScene.Gui.Display(playerInventory, _inventory, ItemType);
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public virtual bool CanAcceptItems(string item,  int count = 1) => _inventory.CountItem(item) < 200;
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
}