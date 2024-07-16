using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.components;
using Necromation.components.animation;
using Necromation.interfaces;

public class Manaforge : Building, IInteractable, ITransferTarget, FurnaceAnimationComponent.IAnimatable
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
    
    /**************************************************************************
     * Private Variables                                                      *
     **************************************************************************/
    private readonly Inventory _inventory = new ManaforgeInventory();
    private readonly FuelComponent _fuelComponent;
    private readonly PowerSourceComponent _powerSourceComponent;
    
    /**************************************************************************
     * Constructor                                                            *
     **************************************************************************/
    public Manaforge() : base()     
    {
        _fuelComponent = new FuelComponent() {InputInventory = _inventory};
        AddComponent(_fuelComponent);
        _powerSourceComponent = new PowerSourceComponent { FuelComponent = _fuelComponent };
        AddComponent(_powerSourceComponent);
        var animation = new FurnaceAnimationComponent(this);
        AddComponent(animation);
    }

    /**************************************************************************
     * Godot Methods                                                          *
     **************************************************************************/
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _fuelComponent._Process(delta);
        _powerSourceComponent._Process(delta);
        
        if (_fuelComponent.FuelTime > 0)
        {
            StartAnimation?.Invoke();
        }
        else
        {
            StopAnimation?.Invoke();
        }
    }
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public override float GetProgressPercent()
    {
        return _fuelComponent.FuelTime / FuelComponent.CoalBurnTime;
    }

    public void Interact(Inventory playerInventory)
    {
        ManaforgeGui.Display(playerInventory, this);
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) && GetOccupiedPositions(position)
            .Any(mapPos => Globals.FactoryScene.TileMap.GetResourceType(mapPos) == "Mana");
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