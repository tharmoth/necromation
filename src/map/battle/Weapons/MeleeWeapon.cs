using System.Linq;
using Godot;

namespace Necromation.map.battle.Weapons;

public class MeleeWeapon : Weapon
{
    public MeleeWeapon(Unit wielder, int range, int damage) : base(wielder, range, damage, 1)
    {
    }


    protected override void Attack(Unit target)
    {
        if (GD.Randf() > 0.5) target.Damage(Damage);
    }
}