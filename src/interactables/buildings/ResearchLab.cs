using System.Collections.Generic;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class ResearchLab : Building, ITransferTarget, IInteractable
{
    private Inventory _inventory = new();
    
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Research Lab";

    private bool _isResearching;
    private double _researchedAmount = 0;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Globals.CurrentTechnology == null)
        {
            _isResearching = false;
            return;
        }

        if (_isResearching)
        {
            _researchedAmount += delta / 30;
            Globals.CurrentTechnology.Progress += delta / 30;
            if (_researchedAmount < 1.0f) return;
            _isResearching = false;
            _researchedAmount = 0;
            return;
        }

        if (!GetItems().Contains("Experiment")) return;
        Remove("Experiment");
        _isResearching = true;
    }

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
        return item == "Experiment" && GetInventories().TrueForAll(inventory => inventory.CountItem(item) < 10);
    }
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
}