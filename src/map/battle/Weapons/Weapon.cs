using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.map.battle.Weapons;

public abstract  class Weapon
{
    protected readonly int Damage;
    protected readonly int Range;
    protected readonly double Cooldown = 1;
    public readonly string Name;
    public readonly int Hands = 1;
    
    private Unit _target;

    protected Weapon(string name, int range, int damage, int hands, int cooldown)
    {
        Name = name;
        Range = range;
        Damage = damage;
        Cooldown = cooldown;
        Hands = hands;
    }

    public void Attack(Unit wielder)
    {
        Attack(wielder, _target);
        wielder.Jiggle();
        wielder.Cooldown = -1 * Cooldown * Battle.TimeStep;
    }
    
    protected abstract void Attack(Unit wielder, Unit target);

    public bool CanAttackDeclump(Unit wielder, List<Unit> targets)
    {
        return CanAttack(wielder, targets, Range > 5 ? Range - 5 : Range);
    }
    
    public bool CanAttack(Unit wielder, List<Unit> targets)
    {
        return CanAttack(wielder, targets, Range);
    }
    
    protected virtual bool CanAttack(Unit wielder, List<Unit> targets, int range)
    {
        var targetCount = targets
            .Count(unit => unit.CachedPosition.DistanceTo(wielder.CachedPosition) <= range * 32);
        if (targetCount == 0)
        {
            _target = null;
        }
        else
        {
            _target = targets
                .Where(unit => unit.CachedPosition.DistanceTo(wielder.CachedPosition) <= range * 32)
                .ElementAt(GD.RandRange(0, targetCount - 1));
        }
        
        return _target != null;
    }
}