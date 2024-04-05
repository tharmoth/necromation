using System.Linq;
using Godot;
using Necromation.battle;
using Necromation.sk;

namespace Necromation.map.battle.Weapons;

public class MeleeWeapon : Weapon
{
    private static readonly PackedScene SlashParticles = GD.Load<PackedScene>("res://src/battle/SlashParticles.tscn");
    
    public MeleeWeapon(string name, int range, int damage, int hands, int cooldown, int ammo) : base(name, range, damage, hands, cooldown, ammo)
    {
    }

    protected override void Attack(Unit wielder, Unit target)
    {
        if (!(GD.Randf() > 0.5)) return;
        
        // var timer = new Timer();
        // timer.WaitTime = Cooldown / 2;
        // timer.Timeout += () =>
        // {
        //     timer.QueueFree();
        wielder.PlayAttackAnimation();
        if (Globals.UnitManager.IsUnitAlive(wielder) && Globals.UnitManager.IsUnitAlive(target)) ApplyDamage(wielder, target);
        // };
        // wielder.SpriteHolder.AddChild(timer);
        // timer.Start();
    }
    
    private void ApplyDamage(Unit wielder, Unit target)
    {
        var armor = target.Armor.Sum(armor => armor.Protection);
        var armorRoll = Utils.RollDice("2d6");
        var damageRoll = Utils.RollDice("2d6");

        var damage = Damage + Mathf.FloorToInt(wielder.Strength * (Hands == 2 ? 1.25 : 1));
        var adjustedDamage = damage - armor + damageRoll - armorRoll;
        adjustedDamage = Mathf.Max(adjustedDamage, 0);
        
        target.Damage(wielder, adjustedDamage);
        if (adjustedDamage <= 0) return;
        target.PlayHitSound();
        
        var rotation = Mathf.RadToDeg(wielder.GlobalPosition.AngleToPoint(target.GlobalPosition));
        
        PlayFX(target.GlobalPosition, rotation);
    }

    private void PlayFX(Vector2 position, float rotation)
    {
        var particles = SlashParticles.Instantiate<GpuParticles2D>();
        particles.Position = position;
        particles.OneShot = true;
        particles.Emitting = true;
        particles.RotationDegrees = rotation - 90;
        Globals.BattleScene.AddChild(particles);
    }
}