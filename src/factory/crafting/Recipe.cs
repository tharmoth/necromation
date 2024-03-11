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
        var max = int.MaxValue;
        foreach (var (type, amount) in Ingredients)
        {
            max = Math.Min(max, inventory.CountItem(type) / amount);
        }
        return max;
    }
}