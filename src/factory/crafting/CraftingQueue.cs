using System.Collections.Generic;
using Godot;

namespace Necromation.gui;

public partial class CraftingQueue : Node
{
    private readonly Queue<Recipe> _queue = new();
    private readonly Inventory _inventory = new();
    
    private double _time;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Globals.CraftingQueue = this;
    }

    public void AddRecipe(Recipe recipe)
    {
        //Add the ingrediants from the player inventory to the crafting queue
        var playerInventory = Globals.PlayerInventory;
        foreach (var (type, amount) in recipe.Ingredients)
        {
            var success = Inventory.TransferItem(playerInventory, _inventory, type, amount);
            if (!success) return;
        }
        _queue.Enqueue(recipe);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_queue.Count == 0) return;
        
        _time += delta;

        if (!(_time > .5)) return;
        
        _time = 0;
        var recipe = _queue.Dequeue();
        recipe.Craft(_inventory, Globals.PlayerInventory);
        MusicManager.PlayCraft();
    }
}