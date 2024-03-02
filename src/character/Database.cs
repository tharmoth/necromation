using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Necromation.gui;
using Necromation.map.battle.Weapons;
using FileAccess = Godot.FileAccess;

namespace Necromation.character;

public class Database
{
    public readonly IReadOnlyList<Recipe> Recipes = LoadRecipes();
    public readonly IReadOnlyList<Technology> Technologies = LoadTechnologies();
    public readonly IReadOnlyList<object> Equpment = LoadEquipment();
    public readonly IReadOnlyList<UnitDef> Units = LoadUnitDefs();
    
    public readonly List<Recipe> UnlockedRecipes = new();
    public readonly List<Recipe> ResearchedTechnologies = new();
    
    private readonly Dictionary<string, Texture2D> _textureCache = new();

    
    public Database()
    {
        List<string> lockedRecipes = new();
        foreach (var technology in Technologies)
        {
            lockedRecipes.AddRange(technology.Unlocks);
        }
        UnlockedRecipes.AddRange(Recipes.Where(recipe => !lockedRecipes.Contains(recipe.Name)).ToList());
        // UnlockedRecipes.AddRange(Recipes);
    }

    #region Recipes
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
    
    public Recipe GetRecipe(string name)
    {
        return Recipes.FirstOrDefault(recipe => recipe.Name == name);
    }
    #endregion

    #region Technology
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
        var prerequisites = recipeDict["prerequisites"]
            .As<Godot.Collections.Array<string>>()
            .ToList();
        var description = recipeDict.TryGetValue("description", out var descriptionVariant) ? descriptionVariant.As<string>() : "None";

        return new Technology(name, count, ingredients, unlocks, prerequisites, description);
    }
    #endregion
    
    #region Equipment
    private static List<object> LoadEquipment()
    {
        var dict = LoadJson("res://res/data/units.json");
        if (dict != null)
        {
            return dict["equipment"].As<Godot.Collections.Array>()
                .Select(x => x.As<Godot.Collections.Dictionary<string, Variant>>())
                .Select(CreateWeaponFromDict)
                .ToList();
        }
        GD.PrintErr("Failed to load equpment!");
        return new List<object>();
    }
    
    private static object CreateWeaponFromDict(Godot.Collections.Dictionary<string, Variant> dict)
    {
        var name = dict["name"].As<string>();
        var type = dict["type"].As<string>();
        var damage = dict.TryGetValue("damage", out var damageV) ? damageV.As<int>() : 1;
        var range =  dict.TryGetValue("range", out var rangeVariant) ? rangeVariant.As<int>() : 1;
        var protection =  dict.TryGetValue("protection", out var protectionVariant) ? protectionVariant.As<int>() : 1;
        var weight =  dict.TryGetValue("weight", out var weightVariant) ? weightVariant.As<int>() : 1;
        var hands = dict.TryGetValue("hands", out var handsVariant) ? weightVariant.As<int>() : 1;
        

        return type switch
        {
            "melee" => new MeleeWeapon(name, range, damage, hands, 1),
            "ranged" => new RangedWeapon(name, range, damage, hands, 6),
            "armor" => new Armor(name, protection, weight),
            _ => null
        };
    }
    #endregion

    #region Units

    private static List<UnitDef> LoadUnitDefs()
    {
        var dict = LoadJson("res://res/data/units.json");
        if (dict != null)
        {
            return dict["units"].As<Godot.Collections.Array>()
                .Select(x => x.As<Godot.Collections.Dictionary<string, Variant>>())
                .Select(CreateUnitDefFromDict)
                .ToList();
        }
        GD.PrintErr("Failed to load units!");
        return new List<UnitDef>();
    }
    
    private static UnitDef CreateUnitDefFromDict(Godot.Collections.Dictionary<string, Variant> dict)
    {
        var name = dict["name"].As<string>();
        return new UnitDef
        {
            Name = name,
            Weapons = dict.TryGetValue("weapons", out var weapons) ? weapons.As<Godot.Collections.Array<string>>().ToList() : new List<string>(),
            Armor = dict.TryGetValue("armor", out var armor) ? armor.As<Godot.Collections.Array<string>>().ToList() : new List<string>()
        };
    }
    
    public class UnitDef
    {
        public string Name;
        public List<string> Weapons;
        public List<string> Armor;
    }
    #endregion


    /******************************************************************
     * Shared Functions                                               *
     ******************************************************************/
    public Texture2D GetTexture(string name, string type="")
    {
        if (_textureCache.TryGetValue(name+type, out var texture)) return texture;

        var path = "res://res/sprites/" + name + ".png";
        if (type != "")  path = "res://res/sprites/" + type + "/" + name + ".png";

        if (!FileAccess.FileExists(path))
        {
            var dirs = DirAccess.Open("res://res/sprites/");
            foreach (var directory in dirs.GetDirectories())
            {
                path = "res://res/sprites/" + directory + "/" + name + ".png";
                if (FileAccess.FileExists(path)) break;
            }
        }

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr("Failed to load texture: " + name);
            return new Texture2D();
        }
        
        texture = GD.Load<Texture2D>(path);
        _textureCache[name+type] = texture;
        return texture;
    }
    
    private static Godot.Collections.Dictionary<string, Variant> LoadJson(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var text = file.GetAsText();
        return Json.ParseString(text).As<Godot.Collections.Dictionary<string, Variant>>();
    }

    public static AudioStream LoadAudioFileFromFolder(string folder)
    {
        var files = DirAccess.Open("res://res/sfx/" + folder + "/").GetFiles().Where(file => file.GetExtension() == "wav" || file.GetExtension() == "mp3")
            .ToArray();
        var file = files[GD.RandRange(0, files.Length - 1)];
        return GD.Load<AudioStream>("res://res/sfx/death/" + file);
    }
}