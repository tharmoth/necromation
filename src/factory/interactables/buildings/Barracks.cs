using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interfaces;
using Necromation.map.character;

namespace Necromation;

public class Barracks : Assembler
{
    /**************************************************************************
     * Hardcoded Scene Imports 											      *
     **************************************************************************/
    public static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/soul_storm.tscn");
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    private Commander _commander;
    
    /**************************************************************************
     * Data Constants                                                         *
     **************************************************************************/
    private const int MaxInputItems = 50;

    public Barracks(string category) : base("Barracks", category)
    {
        Sprite.AddChild(Scene.Instantiate<GpuParticles2D>());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_commander == null || _commander.IsDead) SpawnCommander();
    }

    protected override bool MaxOutputItemsReached()
    {
        return _outputInventory.CountItem(GetRecipe().Products.First().Key) > GetRecipe().Products.First().Value * 200;
    }
    
    private void SpawnCommander()
    {
        _commander = new Commander(Globals.MapScene.FactoryProvince, "Player");
        Globals.MapScene.AddCommander(_commander, Globals.MapScene.FactoryProvince);
        _outputInventory = _commander.Units;
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public override void Interact(Inventory playerInventory)
    {
        if (GetRecipe() == null)
        {
            RecipeSelectionGui.Display(playerInventory, this);
        }
        else
        {
            BarracksGui.Display(playerInventory, this);
        }
        
    }
    #endregion
    
    private class BarracksInventory : Inventory
    {
        public override int GetMaxTransferAmount(string itemType)
        { 
            return itemType.Contains("Skeleton") ? MaxInputItems - CountAllItems() : 0;
        }
    }
}