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
        int count = 1,
        string category = "None"
    )
    {
        Name = name;
        Ingredients = ingredients;
        Count = count;
        Category = category;
    }

    private int Count { get; }
    public string Name { get; }
    public IReadOnlyDictionary<string, int> Ingredients { get; }
    public String Category { get; }
    
    public bool CanCraft(Inventory inventory)
    {
        foreach (var (type, amount) in Ingredients)
        {
            if (inventory.CountItem(type) < amount) return false;
        }

        return true;
    }
    
    public void Craft(Inventory inventory)
    {
        if (!CanCraft(inventory)) return;
        
        foreach (var (type, amount) in Ingredients)
        {
            inventory.RemoveItem(type, amount);
        }
        
        inventory.AddItem(Name, Count);
    }
}