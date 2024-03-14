using System.Collections.Generic;
using Godot;

namespace Necromation.factory.gui;

public abstract partial class DeferredUpdate : Control
{
    // We want to flag dirty in the event that we need to update the inventory.
    // We flag instead of updating immediately to avoid concurrent modification.
    // Caused by new listneres being added to inventories from the update function.
    protected bool Dirty = true;
    protected readonly List<Inventory> Inventories = new List<Inventory>();

    protected void AddUpdateListeners(IEnumerable<Inventory> inventories)
    {
        Inventories.AddRange(inventories);
        foreach (var inventory in Inventories)
        {
            inventory.Listeners.Add(FlagDirty);
        }
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Dirty) Update();
    }

    protected abstract void Update();

    protected void FlagDirty()
    {
        Dirty = true;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        foreach (var inventory in Inventories)
        {
            inventory.Listeners.Remove(FlagDirty);
        }
    }
}