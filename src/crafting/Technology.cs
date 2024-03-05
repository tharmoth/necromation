using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.gui;

public class Technology
{
    public string Name { get; }
    public int Count { get; }
    public IReadOnlyList<string> Ingredients { get; }
    public IReadOnlyList<string> Unlocks { get; }
    public IReadOnlyList<string> Prerequisites { get; }
    public string Description { get; }
    public bool Researched = false;
    private double _progress = 0;

    public double Progress
    {
        get => _progress; 
        set
        {
            _progress = value;
            if (_progress >= Count) Unlock();
        }
    }

    public Technology(
        string name,
        int count,
        IReadOnlyList<string> ingredients,
        IReadOnlyList<string> unlocks,
        IReadOnlyList<string> prerequisites,
        string description = ""
    )
    {
        Name = name;
        Count = count;
        Ingredients = ingredients;
        Unlocks = unlocks;
        Prerequisites = prerequisites;
        Description = description;
    }

    public void Unlock()
    {
        Researched = true;
        GD.Print("Researched " + Name);
        var unlockedRecipes = Database.Instance.Recipes.Where(recipe => Unlocks.Contains(recipe.Name)).ToList();
        Database.Instance.UnlockedRecipes.AddRange(unlockedRecipes);
        Globals.ResearchListeners.ForEach(listener => listener());
        Globals.CurrentTechnology = null;
        Globals.FactoryScene.Gui.TechnologyComplete();
    }
    
    public void Lock()
    {
        Researched = false;
        var lockedRecipes = Database.Instance.Recipes.Where(recipe => Unlocks.Contains(recipe.Name)).ToList();
        foreach (var recipe in lockedRecipes)
        {
            Database.Instance.UnlockedRecipes.Remove(recipe);
        }
        Globals.ResearchListeners.ForEach(listener => listener());
        Globals.CurrentTechnology = null;
        Globals.FactoryScene.Gui.TechnologyComplete();
    }
}