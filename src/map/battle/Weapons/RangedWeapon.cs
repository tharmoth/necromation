using System.Linq;
using Godot;

namespace Necromation.map.battle.Weapons;

public class RangedWeapon : Weapon
{
    public RangedWeapon(Unit wielder, int range, int damage) : base(wielder, range, damage, 6)
    {
    }
    
    protected override void Attack(Unit target)
    {
        var targetLocs = Globals.BattleScene.TileMap.GetTilesInRadius(target.MapPosition, 1).ToList();
        var targetLoc = targetLocs[GD.RandRange(0, targetLocs.Count - 1)];
        Globals.BattleScene.AddChild(new Arrow(targetLoc, Wielder.MapPosition, Damage));
    }
}