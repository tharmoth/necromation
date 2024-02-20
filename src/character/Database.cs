using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Necromation.gui;
using FileAccess = Godot.FileAccess;

namespace Necromation.character;

public class Database
{
    public readonly IReadOnlyList<Recipe> Recipes = LoadRecipes();
    public readonly IReadOnlyList<Technology> Technologies = LoadTechnologies();
    
    public readonly List<Recipe> UnlockedRecipes = new();
    public readonly List<Recipe> ResearchedTechnologies = new();
    
    public Database()
    {
        List<string> lockedRecipes = new();
        foreach (var technology in Technologies)
        {
            lockedRecipes.AddRange(technology.Unlocks);
        }
        UnlockedRecipes.AddRange(Recipes.Where(recipe => !lockedRecipes.Contains(recipe.Name)).ToList());
    }

    private static List<Recipe> LoadRecipes()
    {
        var dict = LoadJson("res://res/data/recipes.json");
        if (dict != null)
        {
            return dict["recipes"].As<Godot.Collections.Array>()
                .Select(x => x.As<Godot.Collections.Dictionary<string, Variant>>())
                .Select(CreateRecipeFromDict)
                .ToList();
        }
        GD.PrintErr("Failed to load recipes!");
        return new List<Recipe>();
    }
    
    private static List<Technology> LoadTechnologies()
    {
        var dict = LoadJson("res://res/data/technology.json");
        if (dict != null)
        {
            return dict["technology"].As<Godot.Collections.Array>()
                .Select(x => x.As<Godot.Collections.Dictionary<string, Variant>>())
                .Select(CreateTechnologyFromDict)
                .ToList();
        }
        GD.PrintErr("Failed to load technology!");
        return new List<Technology>();
    }
    
    private static Godot.Collections.Dictionary<string, Variant> LoadJson(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var text = file.GetAsText();
        return Json.ParseString(text).As<Godot.Collections.Dictionary<string, Variant>>();
    }
    
    private static Recipe CreateRecipeFromDict(Godot.Collections.Dictionary<string, Variant> recipeDict)
    {
        var name = recipeDict["name"].As<string>();
        var ingredients = recipeDict["ingredients"]
            .As<Godot.Collections.Dictionary<string, int>>()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var products = recipeDict["products"]
            .As<Godot.Collections.Dictionary<string, int>>()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var time = recipeDict.TryGetValue("time", out var timeVariant) ? timeVariant.As<float>() : 1.0f;
        var category = recipeDict.TryGetValue("category", out var categoryVariant) ? categoryVariant.As<string>() : "None";

        return new Recipe(name, ingredients, products, category, time);
    }
    
    private static Technology CreateTechnologyFromDict(Godot.Collections.Dictionary<string, Variant> recipeDict)
    {
        var name = recipeDict["name"].As<string>();
        var count = recipeDict["count"].As<int>();
        var ingredients = recipeDict["ingredients"]
            .As<Godot.Collections.Array<string>>()
            .ToList();
        var unlocks = recipeDict["unlocks"]
            .As<Godot.Collections.Array<string>>()
            .ToList();
        var prerequisites = recipeDict["unlocks"]
            .As<Godot.Collections.Array<string>>()
            .ToList();

        return new Technology(name, count, ingredients, unlocks, prerequisites);
    }

    public Texture2D GetTexture(string name)
    {
        var path = "res://res/sprites/" + name + ".png";
        var texture = GD.Load<Texture2D>(path);
        if (texture == null) throw new FileNotFoundException(path);
        return texture;
    }

}