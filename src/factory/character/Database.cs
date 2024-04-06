using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Necromation;
using Necromation.battle.Weapons;
using Necromation.gui;
using Necromation.map.battle.Weapons;
using FileAccess = Godot.FileAccess;

public class Database
{
    public readonly IReadOnlyList<Recipe> Recipes = LoadRecipes();
    public readonly IReadOnlyList<Technology> Technologies = LoadTechnologies();
    public readonly IReadOnlyList<object> Equpment = LoadEquipment();
    public readonly IReadOnlyList<UnitDef> Units = LoadUnitDefs();
    public readonly IReadOnlyDictionary<string, Item> Items = LoadItems();
    
    public readonly List<Recipe> UnlockedRecipes = new();
    
    private readonly Dictionary<string, Texture2D> _textureCache = new();
    
    public static Database Instance = new Database();
    
    private Database()
    {
        List<string> lockedRecipes = new();
        foreach (var technology in Technologies)
        {
            lockedRecipes.AddRange(technology.Unlocks);
        }
        UnlockedRecipes.AddRange(Recipes.Where(recipe => !lockedRecipes.Contains(recipe.Name)).ToList());
        // UnlockedRecipes.AddRange(Recipes);
    }

    #region Items
    private static Dictionary<string, Item> LoadItems()
    {
        var dict = LoadJson("res://res/data/items.json");
        if (dict != null)
        {
            return dict["items"].As<Godot.Collections.Array>()
                .Select(x => x.As<Godot.Collections.Dictionary<string, Variant>>())
                .Select(CreateItemFromDict)
                .ToDictionary(key => key.Name, value => value);
        }
        GD.PrintErr("Failed to load items!");
        return new Dictionary<string, Item>();
    }
    
    private static Item CreateItemFromDict(Godot.Collections.Dictionary<string, Variant> dict)
    {
        var name = dict["name"].As<string>();
        return new Item
        {
            Name = name,
            Description = dict.TryGetValue("description", out var desc) ? desc.As<string>() : "None",
            Category = dict.TryGetValue("category", out var cat) ? cat.As<string>() : "None"
        };
    }
    
    public class Item
    {
        public string Name;
        public string Description;
        public string Category;
    }

    public int CompareItems(string itemTypeA, string itemTypeB)
    {
        if (itemTypeA != null && itemTypeB != null && Items.TryGetValue(itemTypeA, out var aItem) &&
            Items.TryGetValue(itemTypeB, out var bItem))
        {
            var categoryComparison = string.Compare(aItem.Category, bItem.Category, StringComparison.Ordinal);
            if (categoryComparison != 0) return categoryComparison;
        }
            
        return string.Compare(itemTypeA, itemTypeB, StringComparison.Ordinal);
    }
    #endregion
    

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
        var hands = dict.TryGetValue("hands", out var handsVariant) ? handsVariant.As<int>() : 1;
        var ammo = dict.TryGetValue("ammo", out var ammoVariant) ? ammoVariant.As<int>() : -1;

        return type switch
        {
            "melee" => new MeleeWeapon(name, range, damage, hands, 1, ammo),
            "ranged" => new RangedWeapon(name, range, damage, hands, 6, ammo),
            "magic" => new MagicWeapon(name, range, damage, hands, 6, ammo),
            "armor" => new Armor(name, protection, weight),
            _ => null
        };
    }
    #endregion

    #region Units

    public bool IsUnit(string itemType) => Units.Any(unit => unit.Name == itemType);
    
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
            Armor = dict.TryGetValue("armor", out var armor) ? armor.As<Godot.Collections.Array<string>>().ToList() : new List<string>(),
            Mount = dict.TryGetValue("mount", out var mount) ? mount.AsString() : "None",
            Health = dict.TryGetValue("health", out var health) ? health.As<int>() : 10
        };
    }
    
    public class UnitDef
    {
        public string Name;
        public List<string> Weapons;
        public List<string> Armor;
        public string Mount = "None";
        public int Health = 10;
    }
    #endregion


    /******************************************************************
     * Shared Functions                                               *
     ******************************************************************/
    public Texture2D GetTexture(string name, string type="")
    {
        // We have to look for the .import for dumb build reasons.
        if (_textureCache.TryGetValue(name+type, out var texture)) return texture;

        var path = "res://res/sprites/" + name + ".png.import";
        if (type != "")  path = "res://res/sprites/" + type + "/" + name + ".png.import";

        if (!FileAccess.FileExists(path))
        {
            var dirs = DirAccess.Open("res://res/sprites/");
            foreach (var directory in dirs.GetDirectories())
            {
                path = "res://res/sprites/" + directory + "/" + name + ".png.import";
                if (FileAccess.FileExists(path)) break;
            }
        }

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr("Failed to load texture: " + name);
            return type == "unit" ? GetTexture("Awoken Skeleton") : GetTexture("missing");
        }
        
        texture = GD.Load<Texture2D>(path.Replace(".import", ""));
        _textureCache[name+type] = texture;
        return texture;
    }
    
    public Node2D GetParticles(string name)
    {
        var path = "res://res/particles/" + name + ".tscn";
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr("Failed to load particles: " + name);
            return new Node2D();
        }
        return GD.Load<PackedScene>(path).Instantiate<Node2D>();
    }
    
    private static Godot.Collections.Dictionary<string, Variant> LoadJson(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var text = file.GetAsText();
        return Json.ParseString(text).As<Godot.Collections.Dictionary<string, Variant>>();
    }

    public static AudioStream LoadAudioFileFromFolder(string folder)
    {
        // We have to look for the .import for dumb build reasons.
        var files = DirAccess.Open("res://res/sfx/" + folder + "/").GetFiles().Where(filey => filey.Contains("wav.import") || filey.Contains("mp3.import"))
            .ToArray();
        var file = files[GD.RandRange(0, files.Length - 1)].Replace(".import", "");
        return GD.Load<AudioStream>("res://res/sfx/death/" + file);
    }
    
    
    #region Save/Load
    /******************************************************************
     * Save/Load                                                      *
     ******************************************************************/
    public static Godot.Collections.Dictionary<string, Variant> Save()
    {
        var dict = new Godot.Collections.Dictionary<string, Variant>
        {
            ["ItemType"] = "Technology",
            ["CurrentTech"] = Globals.CurrentTechnology != null ? Globals.CurrentTechnology.Name : "None",
            ["Souls"] = Globals.Souls
        };
        for (var i = 0; i < Instance.Technologies.Count; i++)
        {
            dict["Technology" + i] = Instance.Technologies[i].Save();
        }
        return dict;
    }

    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        int index = 0;
        while (nodeData.ContainsKey("Technology" + index))
        {
            var node = (Godot.Collections.Dictionary<string, Variant>) nodeData["Technology" + index];
            var technology = Instance.Technologies.FirstOrDefault(tech => tech.Name == node["Name"].ToString());
            technology?.Load(node);
            index++;
        }
        
        var currentTech = nodeData.TryGetValue("CurrentTech", out var current) ? current.As<string>() : null;
        Globals.CurrentTechnology = Instance.Technologies.FirstOrDefault(tech => tech.Name == currentTech);
        Globals.Souls = nodeData.TryGetValue("Souls", out var current2) ? current2.As<int>() : 0;
    }
    #endregion
}