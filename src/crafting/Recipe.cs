using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Necromation.gui;

public class Recipe
{
    public Recipe(
        string name,
        IReadOnlyDictionary<string, int> ingredients,
        IReadOnlyDictionary<string, int> products,
        string category = "None"
    )
    {
        Name = name;
        Ingredients = ingredients;
        Products = products;
        Category = category;
    }

    public string Name { get; }
    public IReadOnlyDictionary<string, int> Ingredients { get; }
    
    public IReadOnlyDictionary<string, int> Products { get; }
    public String Category { get; }
    
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
            inventory.RemoveItem(type, amount);
        }
        
        foreach (var (type, amount) in Products)
        {
            outputInventory.AddItem(type, amount);
        }
    }
}