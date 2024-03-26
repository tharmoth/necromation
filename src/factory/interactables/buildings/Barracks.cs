using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interfaces;
using Necromation.map.character;

namespace Necromation;

public class Barracks : Building, ITransferTarget, IInteractable
{
    /**************************************************************************
     * Building Implementation                                                *
     **************************************************************************/
    public override string ItemType { get; }
    public override Vector2I BuildingSize => Vector2I.One * 3;
    
    /**************************************************************************
     * Hardcoded Scene Imports 											      *
     **************************************************************************/
    public static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/soul_storm.tscn");
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    public Commander Commander { get; private set; }
    
    // Initialize inventory for loading. it will be discarded.
    private Inventory _inventory = new();
    public Inventory Inventory => _inventory;
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private readonly GpuParticles2D _particles;

    public Barracks() : base()
    {
        _particles = Scene.Instantiate<GpuParticles2D>();
        Sprite.AddChild(_particles);
        ItemType = "Barracks";
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!GodotObject.IsInstanceValid(Commander)) SpawnCommander();
        if (GetProgressPercent() > 0 && !_particles.Emitting) _particles.Emitting = true;
        else if (GetProgressPercent() <= 0 && _particles.Emitting) _particles.Emitting = false;
    }
    
    private void SpawnCommander()
    {
        Commander = Globals.MapScene.Commanders.FirstOrDefault(commander => commander.BarracksId == Id) 
                     ?? new Commander(Globals.MapScene.FactoryProvince, "Player");
        Commander.BarracksId = Id;
        _inventory = Commander.Units;
    }
    
    public override void Remove(Inventory to, bool quietly = false)
    {
        base.Remove(to, quietly);
        Commander.Kill();
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public void Interact(Inventory playerInventory)
    {
        // if (GetRecipe() == null)
        // {
        //     RecipeSelectionGui.Display(playerInventory, this);
        // }
        // else
        // {
            BarracksGui.Display(playerInventory, this);
        // }
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
    #endregion
}