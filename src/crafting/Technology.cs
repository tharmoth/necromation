using System.Collections.Generic;

namespace Necromation.gui;

public class Technology
{
    public string Name { get; }
    public int Count { get; }
    public IReadOnlyList<string> Ingredients { get; }
    public IReadOnlyList<string> Unlocks { get; }
    public IReadOnlyList<string> Prerequisites { get; }
    
    public Technology(
        string name,
        int count,
        IReadOnlyList<string> ingredients,
        IReadOnlyList<string> unlocks,
        IReadOnlyList<string> prerequisites
    )
    {
        Name = name;
        Count = count;
        Ingredients = ingredients;
        Unlocks = unlocks;
        Prerequisites = prerequisites;
    }
}