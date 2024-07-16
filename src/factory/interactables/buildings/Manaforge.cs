using System;
using System.Collections.Generic;
using Godot;
using Necromation;
using Necromation.components;
using Necromation.interfaces;

public class Manaforge : Building, IInteractable, ITransferTarget, IPowerSource, FurnaceAnimationComponent.IAnimatable
{
    /**************************************************************************
     * Events                                                                 *
     **************************************************************************/
    public event Action StartAnimation;
    public event Action StopAnimation;
    
    /**************************************************************************
     * Data Constants                                                         *
     **************************************************************************/
    private const int MaxInputItems = 50;
    
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    #region Building Implementation
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Manaforge";
    #endregion

    #region IPowerSource Implementation
    public float Energy { get; set; }
    public float EnergyMax => 1000.0f;
    public float Power => 100.0f;
    public bool Disconnected
    {
        set
        {
            GD.Print($"Manaforge Disconnected: {value}");
        }
    }
    #endregion
    
    /**************************************************************************
     * Private Variables                                                      *
     **************************************************************************/
    private readonly Inventory _inventory = new ManaforgeInventory();
    private readonly FuelComponent _fuelComponent;
    
    public Manaforge() : base()
    {
        _fuelComponent = new FuelComponent() {InputInventory = _inventory};
        new FurnaceAnimationComponent(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _fuelComponent._Process(delta);
        if (_fuelComponent.CanDrawPower() && Energy <= EnergyMax)
        {
            _fuelComponent.DrawPower();
            Energy += Power * (float) delta;
            StartAnimation?.Invoke();
        }
        else
        {
            StopAnimation?.Invoke();
        }
    }
    
    public override float GetProgressPercent()
    {
        return _fuelComponent.FuelTime / FuelComponent.CoalBurnTime;
    }

    public void Interact(Inventory playerInventory)
    {
        ManaforgeGui.Display(playerInventory, this);
    }
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    private class ManaforgeInventory : Inventory
    {
        public override int GetMaxTransferAmount(string itemType)
        {
            var currentCount = CountItem(itemType);
            if (IsFuel(itemType) && currentCount < MaxInputItems)
            {
                return MaxInputItems - currentCount;
            }

            return 0;
        }
    }
    
    private static bool IsFuel(string item) => item == "Coal Ore";
    public bool CanAcceptItems(string item, int count = 1) => _inventory.CanAcceptItems(item, count);
    public bool CanAcceptItemsInserter(string item, int count = 1) => _inventory.CanAcceptItemsInserter(item, count);
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public List<string> GetItems() => _inventory.GetItems();
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<Inventory> GetInventories() => new() { _inventory };
    public int GetMaxTransferAmount(string itemType) => _inventory.GetMaxTransferAmount(itemType);
    #endregion
}