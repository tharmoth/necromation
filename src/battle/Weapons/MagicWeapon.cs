using System.Linq;
using Godot;
using Necromation.map.battle;
using Necromation.map.battle.Weapons;
using Necromation.sk;

namespace Necromation.battle.Weapons;

public class MagicWeapon : Weapon
{
    
    private static readonly PackedScene _particles = GD.Load<PackedScene>("res://src/battle/Weapons/explosion.tscn");
    
    public MagicWeapon(string name, int range, int damage, int hands, float cooldown, int ammo) : base(name, range, damage, hands, cooldown, ammo)
    {
    }

    protected override void Attack(Unit wielder, Unit target)
    {
        wielder.PlayAttackAnimation();
        var targetLoc = target.MapPosition;
        
        Globals.BattleScene.AddChild(new Projectile(wielder.MapPosition, targetLoc, mapPosition => ApplyDamage(wielder, mapPosition), "fireball"));
    }

    private void ApplyDamage(Unit wielder, Vector2I mapPosition)
    {
        PlayHitSound(mapPosition);
        var zoneOfDoom = Globals.BattleScene.TileMap.GetTilesInRadius(mapPosition, 3);
        foreach (var position in zoneOfDoom)
        {
            var target =
                Globals.BattleScene.TileMap.GetEntity(position,
                    BattleTileMap.Unit) as Unit;
            if (target == wielder) continue;
            target?.Damage(wielder, 100);
        }

        var particles = _particles.Instantiate<GpuParticles2D>();
        particles.Position = Globals.BattleScene.TileMap.MapToGlobal(mapPosition);
        particles.Emitting = true;
        particles.OneShot = true;
        particles.Position += new Vector2((float)GD.RandRange(-32.0f, 32.0f), (float)GD.RandRange(-32.0f, 32.0f));
        Globals.BattleScene.AddChild(particles);
        
        foreach (var gpuParticles2D in particles.GetChildren().OfType<GpuParticles2D>())
        {
            gpuParticles2D.OneShot = true;
        }

    }
    
    private void PlayHitSound(Vector2I mapPosition)
    {
        var audioplayer = new AudioStreamPlayer2D();
        var randomizer = new AudioStreamRandomizer();
        randomizer.AddStream(0,
            GD.Load<AudioStream>(
                "res://res/sfx/kenny-audio/explosion1.ogg"));
        audioplayer.Stream = randomizer;
        audioplayer.VolumeDb = -20;
        audioplayer.Position = Globals.BattleScene.TileMap.MapToGlobal(mapPosition);
        Globals.BattleScene.AddChild(audioplayer);
        audioplayer.CallDeferred("play");
    }
}