using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.gui;

public class Recipe
{
    public string Name { get; }
    public IReadOnlyDictionary<string, int> Ingredients { get; }
    public IReadOnlyDictionary<string, int> Products { get; }
    public String Category { get; }
    public float Time { get; }
    
    public Recipe(
        string name,
        IReadOnlyDictionary<string, int> ingredients,
        IReadOnlyDictionary<string, int> products,
        string category = "None",
        float time = 1.0f
    )
    {
        Name = name;
        Ingredients = ingredients;
        Products = products;
        Category = category;
        Time = time;
    }

    public Recipe()
    {
        Name = "Missing";
        Ingredients = new Dictionary<string, int>();
        Products = new Dictionary<string, int>();
        Category = "None";
        Time = 1.0f;
    }

    public bool CanCraft(Inventory inventory)
    {
        foreach (var (type, amount) in Ingredients)
        {
            if (inventory.CountItem(type) < amount) return false;
        }

        return true;
    }
    
    public void Craft(Inventory inventory, Inventory outputInventory = null)
    {
        if (!CanCraft(inventory)) return;
        outputInventory ??= inventory;
        
        foreach (var (type, amount) in Ingredients)
        {
            inventory.Remove(type, amount);
        }
        
        foreach (var (type, amount) in Products)
        {
            outputInventory.Insert(type, amount);
        }
    }
    
    public Texture2D GetIcon()
    {
        return Database.Instance.GetTexture(Products.Keys.First());
    }
    
    public int GetMaxCraftable(Inventory inventory)
    {
        // Recursively find the max amount of this recipe that can be crafted if missing ingredients are crafted until
        // the inventory runs out of necessary resources
        if (this is not { Category: "None" or "hands" }) return 0;

        var copy = new Inventory(inventory);
        var max = 0;
        while (CraftRecursive2(copy, this, 1))
        {
            max++;
        }

        return max;
    }

    private bool CraftRecursive(Inventory inventory, Recipe recipe)
    {
        // Go through each ingredient and if the inventory does not have enough, craft the ingredient
        // If the ingredient is not craftable, return false
        foreach (var (type, amount) in recipe.Ingredients)
        {
            if (inventory.CountItem(type) >= amount) continue;

            var subRecipe = Database.Instance.Recipes
                .Where(dRecipe => dRecipe is { Category: "None" or "hands" })
                .FirstOrDefault(dRecipe => dRecipe.Products.ContainsKey(type));

            if (subRecipe is not { Category: "None" or "hands" }) return false;
            while (inventory.CountItem(type) < amount)
            {
                var success = CraftRecursive(inventory, subRecipe);
                if (!success) return false;
            }
        }
        
        // If all ingredients are craftable, craft the recipe
        recipe.Craft(inventory);
        return true;
    }
    
    private bool CraftRecursive2(Inventory sourceInventory, Recipe recipe, int amountToCraft)
    {
        var itemsToQueue = new List<CraftingQueue.CraftingQueueItem>();
        var stack = new Stack<CraftingQueue.CraftingQueueItem>();
        stack.Push(new CraftingQueue.CraftingQueueItem(recipe, amountToCraft));

        while (stack.Count > 0)
        {
            var toAddItem = stack.Pop();
            itemsToQueue.Add(toAddItem);
            
            foreach (var (type, count) in toAddItem.Recipe.Ingredients)
            {
                var toAddCount = count * toAddItem.Count;
                var sourceCount = sourceInventory.CountItem(type);
                var neededCount = Mathf.Max(0, toAddCount - sourceCount);
                Inventory.TransferItem(sourceInventory, toAddItem.Inventory, type, toAddCount - neededCount);
                
                if (neededCount <= 0) continue;
                var subRecipe = Database.Instance.Recipes
                    .Where(dRecipe => dRecipe is { Category: "None" or "hands" })
                    .FirstOrDefault(databaseRecipe => databaseRecipe.Products.ContainsKey(type));
                if (subRecipe is null) return false;
                subRecipe.Products.TryGetValue(type, out var itemsPerCraft);
                
                // Calculate the minimum number of crafts to get the needed amount of items.
                var recipeCraftsNeeded = Mathf.CeilToInt(neededCount / (float) itemsPerCraft);
                var craftingQueueItem = new CraftingQueue.CraftingQueueItem(subRecipe, recipeCraftsNeeded, toAddItem)
                {
                    Extra = recipeCraftsNeeded * itemsPerCraft - neededCount
                };
                stack.Push(craftingQueueItem);
                toAddItem.Children.Add(craftingQueueItem);
            }
        }

        itemsToQueue.Reverse();
        return true;
    }
}