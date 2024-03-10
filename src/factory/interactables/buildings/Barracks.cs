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
     * Visuals Variables 													  *
     **************************************************************************/
    private readonly GpuParticles2D _particles;

    public Barracks(string category) : base("Barracks", category)
    {
        _particles = Scene.Instantiate<GpuParticles2D>();
        Sprite.AddChild(_particles);
        MaxInputItems = 200;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!GodotObject.IsInstanceValid(_commander)) SpawnCommander();
        if (GetProgressPercent() > 0 && !_particles.Emitting) _particles.Emitting = true;
        else if (GetProgressPercent() <= 0 && _particles.Emitting) _particles.Emitting = false;
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
}