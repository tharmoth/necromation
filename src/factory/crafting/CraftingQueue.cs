using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

namespace Necromation.gui;

public partial class CraftingQueue : Node
{
    public readonly List<Action> Listeners = new();
    public ImmutableList<CraftingQueueItem> Queue => _queue.ToImmutableList();
    public double Time => _time;
    public const double TimePerCraft = .5;
    
    private readonly List<CraftingQueueItem> _queue = new();
    private readonly Inventory _inventory = new();
    private double _time;

    /// <summary>
    /// Queues a recipe to be crafted. Takes the items from the player inventory and adds them to the crafting queue.
    /// If the players inventory does not have enough items to craft the recipe, it will only craft as many as it can.
    /// </summary>
    /// <param name="name"> Name of the recipe to be crafted </param>
    /// <param name="count"> Number of times to craft the recipe </param>
    public void QueueRecipe(string name, int count)
    {
        var recipe = Database.Instance.GetRecipe(name);
        var playerInventory = Globals.PlayerInventory;
        var amountToCraft = Mathf.Min(count, recipe.GetMaxCraftable(playerInventory));
        
        if (amountToCraft <= 0) return;
        
        //Add the ingredients from the player inventory to the crafting queue
        foreach (var (type, amount) in recipe.Ingredients)
        {
            Inventory.TransferItem(playerInventory, _inventory, type, amount * amountToCraft);
        }
        _queue.Add(new CraftingQueueItem(recipe.Name, amountToCraft));
        
        Listeners.ForEach(listener => listener());
    }
    
    /// <summary>
    /// Removes the given amount of crafts from the crafting queue item.
    /// If the crafting queue item then has no items it is removed.
    /// </summary>
    /// <param name="item"> Item in the queue to remove crafts from. </param>
    /// <param name="countToCancel"> Number of crafts of that recipe to cancel. </param>
    public void CancelItem(CraftingQueueItem item, int countToCancel)
    {
        var recipe = Database.Instance.GetRecipe(item.RecipeName);
        var playerInventory = Globals.PlayerInventory;
        var amountToCancel = Mathf.Min(countToCancel, item.Count);
        
        item.Count -= countToCancel;
        if (item.Count <= 0) _queue.Remove(item);
        if (amountToCancel <= 0) return;
        
        // Give the player back the canceled items.
        foreach (var (type, amount) in recipe.Ingredients)
        {
            Inventory.TransferItem(_inventory, playerInventory, type, amount * amountToCancel);
        }
        
        MusicManager.PlayCraft();
        Listeners.ForEach(listener => listener());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_queue.Count == 0) return;
        
        _time += delta;

        if (!(_time > TimePerCraft)) return;
        
        _time = 0;
        var craftingQueueItem = _queue[0];
        craftingQueueItem.Count--;
        var recipe = Database.Instance.GetRecipe(craftingQueueItem.RecipeName);
        recipe.Craft(_inventory, Globals.PlayerInventory);
        MusicManager.PlayCraft();
        
        if (craftingQueueItem.Count <= 0) _queue.RemoveAt(0);
        
        Listeners.ForEach(listener => listener());
    }
    
    public class CraftingQueueItem
    {
        public int Index => Globals.FactoryScene.CraftingQueue._queue.ToList().IndexOf(this);
        public readonly string RecipeName;
        public int Count;
        
        public CraftingQueueItem(string recipeName, int count)
        {
            RecipeName = recipeName;
            Count = count;
        }
    }
}