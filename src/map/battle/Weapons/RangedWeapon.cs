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
        var targetLocs = Globals.BattleScene.TileMap.GetTilesInRadius(target.MapPosition, 2).ToList();
        var targetLoc = targetLocs[GD.RandRange(0, targetLocs.Count - 1)];
        Globals.BattleScene.AddChild(new Arrow(targetLoc, Wielder.MapPosition, Damage));
        PlaySound();
    }
    
    private void PlaySound()
    {
        var randomizer = new AudioStreamRandomizer();
        randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/warfare_medieval_english_longbow_arrow_shoot_from_bow_001.mp3"));
        randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/warfare_medieval_english_longbow_arrow_shoot_from_bow_002.mp3"));
        Wielder.Audio.Stream = randomizer;
        Wielder.Audio.VolumeDb = -20;
        Wielder.Audio.Play();
    }
}