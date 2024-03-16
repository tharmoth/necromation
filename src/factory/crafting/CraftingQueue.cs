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
    public const double TimePerCraft = 15;
    
    private readonly List<CraftingQueueItem> _queue = new();
    private readonly Inventory _inventory = new();
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
        
        var subRecipeQueue = new List<CraftingQueueItem>();
        while (amountToCraft > 0)
        {
            amountToCraft--;
            CraftRecursive(playerInventory, recipe, subRecipeQueue);
        }

        _queue.AddRange(subRecipeQueue);
        
        Listeners.ForEach(listener => listener());
    }

    private void testytest(Inventory sourceInventory, Recipe recipe)
    {
        var stack = new Stack<CraftingQueueItem2> {};
        stack.Push(new CraftingQueueItem2(recipe, 1, false));

        while (stack.Count > 0)
        {
            var itemToAdd = stack.Pop();

            foreach (var (item, count) in itemToAdd.Recipe.Ingredients)
            {
                var sourceCount = sourceInventory.CountItem(item);
                if (sourceCount < count)
                {
                    var subRecipe = Database.Instance.Recipes.First(databaseRecipe => databaseRecipe.Products.ContainsKey(item));
                    subRecipe.Products.TryGetValue(item, out var subCount);
                    var itemsNeeded = count - sourceCount;
                    var recipeCraftsNeeded = Mathf.CeilToInt(itemsNeeded / (float) subCount);
                    stack.Push(new CraftingQueueItem2(subRecipe, recipeCraftsNeeded, true));
                    Inventory.TransferItem(sourceInventory, _inventory, item, count - itemsNeeded);
                }
                else
                {
                    Inventory.TransferItem(sourceInventory, _inventory, item, count);
                }
            }
        }
    }

    public class CraftingQueueItem2
    {
        // public int Index => Globals.FactoryScene.CraftingQueue._queue.ToList().IndexOf(this);
        public readonly Recipe Recipe;
        public int Count;
        public bool IsPrerequisite;
        
        public CraftingQueueItem2(Recipe recipe, int count, bool isPrerequisite)
        {
            Recipe = recipe;
            Count = count;
            IsPrerequisite = isPrerequisite;
        }
    }

    private void CraftRecursive(Inventory inventory, Recipe recipe, List<CraftingQueueItem> recipeQueue, CraftingQueueItem parent = null)
    {
        if (!recipeQueue.Select(queueRecipe => queueRecipe.Recipe).Contains(recipe))
        {
            recipeQueue.Insert(0, new CraftingQueueItem(recipe, 1, parent));
        }
        else
        {
            recipeQueue.First(queueRecipe => queueRecipe.Recipe == recipe).Count++;
        }
        
        var subParent = recipeQueue.First(queueRecipe => queueRecipe.Recipe == recipe);
        
        // Go through each ingredient and if the inventory does not have enough, craft the ingredient
        // If the ingredient is not craftable, return false
        foreach (var (type, amount) in recipe.Ingredients)
        {
            var amountInInventory = inventory.CountItem(type);
            
            var amountQueued = 0;
            if (amountInInventory < amount)
            {
                var subRecipe = Database.Instance.GetRecipe(type);
                
                while (amountInInventory + amountQueued < amount)
                {
                    var subQueuedItem = recipeQueue.FirstOrDefault(queueRecipe => queueRecipe.Recipe == recipe);
                    if (subQueuedItem != null && subQueuedItem.Extra > 0)
                    {
                        subQueuedItem.Extra--;
                        amountQueued++;
                    }
                    else
                    {
                        subRecipe.Products.TryGetValue(type, out var subAmount);
                        amountQueued += subAmount;
                        CraftRecursive(inventory, subRecipe, recipeQueue, subParent);
                    }
                }

                if (amountInInventory + amountQueued > amount)
                {
                    recipeQueue.First(queueRecipe => queueRecipe.Recipe == subRecipe).Extra += amountInInventory + amountQueued - amount;
                }
            }

            Inventory.TransferItem(inventory, _inventory, type, amount - amountQueued);
        }
    }
    
    
    /// <summary>
    /// Removes the given amount of crafts from the crafting queue item.
    /// If the crafting queue item then has no items it is removed.
    /// </summary>
    /// <param name="item"> Item in the queue to remove crafts from. </param>
    /// <param name="countToCancel"> Number of crafts of that recipe to cancel. </param>
    public void CancelItem(CraftingQueueItem item, int countToCancel)
    {
        var recipe = item.Recipe;
        var playerInventory = Globals.PlayerInventory;
        var numCraftsToCancel = Mathf.Min(countToCancel, item.Count);
        
        item.Count -= countToCancel;
        if (item.Count <= 0) _queue.Remove(item);
        if (numCraftsToCancel <= 0) return;
        
        // Give the player back the canceled items.
        foreach (var (type, amount) in recipe.Ingredients)
        {
            Inventory.TransferItem(_inventory, playerInventory, type, amount * numCraftsToCancel);
        }

        if (item.Parent != null)
        {
            CancelItem(item.Parent, item.Parent.Count);
            foreach (var (type, amount) in recipe.Products)
            {
                playerInventory.Remove(type, amount * numCraftsToCancel);
            }
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

        recipe.Craft(_inventory, craftingQueueItem.Parent != null ? _inventory : Globals.PlayerInventory);


        if (craftingQueueItem.Count <= 0)
        {
            _queue.RemoveAt(0);
            if (craftingQueueItem.Extra > 0)
            {
                Inventory.TransferItem(_inventory, Globals.PlayerInventory, recipe.Products.First().Key, craftingQueueItem.Extra);
            }
            MusicManager.PlayCraft();
        }
        
        Listeners.ForEach(listener => listener());
    }
    
    public class CraftingQueueItem
    {
        public int Index => Globals.FactoryScene.CraftingQueue._queue.ToList().IndexOf(this);
        public readonly Recipe Recipe;
        public int Count;
        public CraftingQueueItem Parent;
        public int Extra;
        
        public CraftingQueueItem(Recipe recipe, int count, CraftingQueueItem parent = null)
        {
            Recipe = recipe;
            Count = count;
            Parent = parent;
        }
    }
}