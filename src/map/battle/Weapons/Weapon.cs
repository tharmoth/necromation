using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.map.battle.Weapons;

public abstract  class Weapon
{
    protected readonly int Damage;
    protected readonly int Range;
    protected readonly Unit Wielder;
    protected readonly double Cooldown = 1;
    
    private Unit _target;

    protected Weapon(Unit wielder, int range, int damage, int cooldown)
    {
        Wielder = wielder;
        Range = range;
        Damage = damage;
        Cooldown = cooldown;
    }

    public void Attack()
    {
        Attack(_target);
        Wielder.Jiggle();
        Wielder.Cooldown = -1 * Cooldown * Battle.TimeStep;
    }
    protected abstract void Attack(Unit target);
    public bool CanAttack(List<Unit> targets)
    {
        // _target = targets
        //     .Where(unit => unit.GlobalPosition.DistanceTo(Wielder.GlobalPosition) < Range * 32)
        //     .MinBy(unit => Guid.NewGuid());

        var targetCount = targets
            .Count(unit => unit.CachedPosition.DistanceTo(Wielder.CachedPosition) <= Range * 32);
        if (targetCount == 0)
        {
            _target = null;
        }
        else
        {
            _target = targets
                .Where(unit => unit.CachedPosition.DistanceTo(Wielder.CachedPosition) <= Range * 32)
                .ElementAt(GD.RandRange(0, targetCount - 1));
        }
        
        return _target != null;
    }
}