﻿using System;
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
    public readonly int Ammo;
    
    private Unit _target;

    protected Weapon(string name, int range, int damage, int hands, float cooldown, int ammo)
    {
        Name = name;
        Range = range;
        Damage = damage;
        Cooldown = cooldown;
        Hands = hands;
        Ammo = ammo;
    }

    public void Attack(Unit wielder)
    {
        Attack(wielder, _target);
        wielder.Cooldown = Cooldown * BattleScene.TimeStep;
        wielder.Ammo[this]--;
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
    
    public virtual bool CanAttack(Unit wielder, List<Unit> targets, int range)
    {
        if (wielder.Ammo[this] == 0) return false; 
        var targetCount = targets
            .Count(unit => unit.GlobalPosition.DistanceTo(wielder.GlobalPosition) <= range * 32);
        if (targetCount == 0)
        {
            _target = null;
        }
        else
        {
            _target = targets
                .Where(unit => unit.GlobalPosition.DistanceTo(wielder.GlobalPosition) <= range * 32)
                .ElementAt(GD.RandRange(0, targetCount - 1));
        }
        
        return _target != null;
    }
}