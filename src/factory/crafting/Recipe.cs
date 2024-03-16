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

        if (this is not { Category: "None" or "Hand" }) return 0;

        var copy = new Inventory(inventory);
        var max = 0;
        while (CraftRecursive(copy, this))
        {
            max++;
        }
        
        // Old Method
        // var max = int.MaxValue;
        // foreach (var (type, amount) in Ingredients)
        // {
        //     max = Math.Min(max, inventory.CountItem(type) / amount);
        // }
        return max;
    }

    private bool CraftRecursive(Inventory inventory, Recipe recipe)
    {
        // Go through each ingredient and if the inventory does not have enough, craft the ingredient
        // If the ingredient is not craftable, return false
        foreach (var (type, amount) in recipe.Ingredients)
        {
            if (inventory.CountItem(type) >= amount) continue;
            var subRecipe = Database.Instance.GetRecipe(type);
            if (subRecipe is not { Category: "None" or "Hand" }) return false;
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
}