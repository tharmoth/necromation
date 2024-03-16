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
    private double _time;

    /// <summary>
    /// Queues a recipe to be crafted. Takes the items from the player inventory and adds them to the crafting queue.
    /// If the players inventory does not have enough items to craft the recipe, it will only craft as many as it can.
    /// </summary>
    /// <param name="recipe"> The recipe to be crafted </param>
    /// <param name="count"> Number of times to craft the recipe </param>
    public void QueueRecipe(Recipe recipe, int count)
    {
        var playerInventory = Globals.PlayerInventory;
        var amountToCraft = Mathf.Min(count, recipe.GetMaxCraftable(playerInventory));
        
        if (amountToCraft <= 0) return;
        
        CraftRecursive(playerInventory, recipe, amountToCraft);

        Listeners.ForEach(listener => listener());
    }

    private void CraftRecursive(Inventory sourceInventory, Recipe recipe, int amountToCraft)
    {
        var itemsToQueue = new List<CraftingQueueItem>();
        var stack = new Stack<CraftingQueueItem>();
        stack.Push(new CraftingQueueItem(recipe, amountToCraft));

        while (stack.Count > 0)
        {
            var itemToAdd = stack.Pop();
            itemsToQueue.Add(itemToAdd);
            
            foreach (var (item, count) in itemToAdd.Recipe.Ingredients)
            {
                var countToAdd = count * itemToAdd.Count;
                var sourceCount = sourceInventory.CountItem(item);
                var itemsNeeded = Mathf.Max(0, countToAdd - sourceCount);
                Inventory.TransferItem(sourceInventory, itemToAdd.Inventory, item, countToAdd - itemsNeeded);
                
                if (itemsNeeded <= 0) continue;
                var subRecipe = Database.Instance.Recipes.First(databaseRecipe => databaseRecipe.Products.ContainsKey(item));
                subRecipe.Products.TryGetValue(item, out var itemsPerCraft);
                    
                var recipeCraftsNeeded = Mathf.CeilToInt(itemsNeeded / (float) itemsPerCraft);
                var craftingQueueItem = new CraftingQueueItem(subRecipe, recipeCraftsNeeded, itemToAdd)
                {
                    Extra = recipeCraftsNeeded * itemsPerCraft - itemsNeeded
                };
                stack.Push(craftingQueueItem);
                itemToAdd.Children.Add(craftingQueueItem);
            }
        }

        itemsToQueue.Reverse();
        _queue.AddRange(itemsToQueue);
    }

    public class CraftingQueueItem
    {
        // public int Index => Globals.FactoryScene.CraftingQueue._queue.ToList().IndexOf(this);
        public readonly Recipe Recipe;
        public int Count;
        public CraftingQueueItem Parent;
        public List<CraftingQueueItem> Children = new();
        public int Extra;
        public Inventory Inventory = new();
        
        public CraftingQueueItem(Recipe recipe, int count, CraftingQueueItem parent = null)
        {
            Recipe = recipe;
            Count = count;
            Parent = parent;
        }
    }
    
    /// <summary>
    /// Removes the given amount of crafts from the crafting queue item.
    /// If the crafting queue item then has no items it is removed.
    /// </summary>
    /// <param name="craftingQueueItem"> Item in the queue to remove crafts from. </param>
    /// <param name="countToCancel"> Number of crafts of that recipe to cancel. </param>
    public void CancelItem(CraftingQueueItem craftingQueueItem, int countToCancel)
    {
        var recipe = craftingQueueItem.Recipe;
        var playerInventory = Globals.PlayerInventory;
        var numCraftsToCancel = Mathf.Min(countToCancel, craftingQueueItem.Count);
        
        craftingQueueItem.Count -= countToCancel;
        if (craftingQueueItem.Count <= 0) _queue.Remove(craftingQueueItem);
        if (numCraftsToCancel <= 0) return;
        
        // Give the player back the canceled items.
        foreach (var (type, amount) in recipe.Ingredients)
        {
            var amountToTransfer = Math.Min(amount * numCraftsToCancel, craftingQueueItem.Inventory.CountItem(type));
            Inventory.TransferItem(craftingQueueItem.Inventory, playerInventory, type, amountToTransfer);
        }
        
        if (craftingQueueItem.Parent != null)
        {
            CancelItem(craftingQueueItem.Parent, craftingQueueItem.Parent.Count);
        }

        foreach (var childItem in craftingQueueItem.Children)
        {
            childItem.Parent = null;
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
        var recipe = craftingQueueItem.Recipe;

        recipe.Craft(craftingQueueItem.Inventory, craftingQueueItem.Parent == null ? Globals.PlayerInventory : craftingQueueItem.Parent.Inventory);

        if (craftingQueueItem.Count <= 0)
        {
            _queue.RemoveAt(0);
            MusicManager.PlayCraft();
            Inventory.TransferAllTo(craftingQueueItem.Inventory, Globals.PlayerInventory);
        }
        
        Listeners.ForEach(listener => listener());
    }
}