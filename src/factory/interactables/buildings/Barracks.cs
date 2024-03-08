using System.Collections.Generic;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public class Barracks : Building, IInteractable, ITransferTarget
{
    /**************************************************************************
     * Building Implementation                                                *
     **************************************************************************/
    public override string ItemType => "Barracks";
    public override Vector2I BuildingSize => Vector2I.One * 3;
    
    /*
     * Logic Variables
     */
    private BarracksInventory _inventory = new();

    public Barracks()
    {
        Sprite.AddChild(GD.Load<PackedScene>("res://src/factory/interactables/buildings/soul_storm.tscn")
            .Instantiate<GpuParticles2D>());
    }
    public override void _Ready()
    {
        base._Ready();
        Sprite.CallDeferred("add_child",
            GD.Load<PackedScene>("res://src/factory/interactables/buildings/soul_storm.tscn").Instantiate<GpuParticles2D>());
    }

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
    public bool CanAcceptItems(string item, int count = 1) => _inventory.CanAcceptItems(item, count);
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
    
    private class BarracksInventory : Inventory
    {
        public override bool CanAcceptItems(string item, int count = 1)
        {
            return item.Contains("Skeleton") && GetInventories().TrueForAll(inventory => inventory.CountItem(item) < 100);
        }
    }
}