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
        if (!(GD.Randf() > 0.5)) return;
        target.Damage(Damage);
        PlaySound(target);
    }
    
    private void PlaySound(Unit target)
    {
        var randomizer = new AudioStreamRandomizer();
        randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/stab/zapsplat_warfare_sword_blade_tip_stab_dig_into_earth_soil_mud_009_93653.mp3"));
        randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/stab/zapsplat_warfare_sword_stab_into_body_flesh_light_squelch_93748.mp3"));

        target.Audio.Stream = randomizer;
        target.Audio.VolumeDb = -20;
        target.Audio.Play();
    }
}