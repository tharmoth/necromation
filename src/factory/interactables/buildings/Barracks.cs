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
    public Commander Commander { get; private set; }
    
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
        if (!GodotObject.IsInstanceValid(Commander)) SpawnCommander();
        if (GetProgressPercent() > 0 && !_particles.Emitting) _particles.Emitting = true;
        else if (GetProgressPercent() <= 0 && _particles.Emitting) _particles.Emitting = false;
    }
    
    private void SpawnCommander()
    {
        var test = Globals.MapScene.Commanders.Where(commy => commy.Team == "Player").ToList();
        Commander = Globals.MapScene.Commanders.FirstOrDefault(commander => commander.BarracksId == Id) 
                     ?? new Commander(Globals.MapScene.FactoryProvince, "Player");
        Commander.BarracksId = Id;
        _outputInventory = Commander.Units;
    }
    
    protected override bool MaxAutocraftReached()
    {
        return _outputInventory.CountItems() >= Commander.CommandCap;
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
    public override void Interact(Inventory playerInventory)
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
}