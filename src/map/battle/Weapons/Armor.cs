namespace Necromation.map.battle.Weapons;

public class Armor
{
    public readonly int Protection;
    public readonly int Weight;
    public readonly string Name;
    
    public Armor(string name, int protection, int weight)
    {
        Name = name;
        Protection = protection;
        Weight = weight;
    }
}