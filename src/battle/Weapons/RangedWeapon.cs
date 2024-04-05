using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.sk;

namespace Necromation.map.battle.Weapons;

public class RangedWeapon : Weapon
{
    public RangedWeapon(string name, int range, int damage, int hands, float cooldown, int ammo) : base(name, range, damage, hands, cooldown, ammo)
    {

    }
    
    protected override void Attack(Unit wielder, Unit target)
    {
        wielder.PlayAttackAnimation();
        // var targetLocs = Globals.BattleScene.TileMap.GetTilesInRadius(target.MapPosition, 0).ToList();
        // var targetLoc = targetLocs[GD.RandRange(0, targetLocs.Count - 1)];
        var startGlobal = Globals.BattleScene.TileMap.MapToGlobal(wielder.MapPosition);
        
        var targetLoc = target.MapPosition;

        // Rudimentary leading of target
        var difference = startGlobal - target.GlobalPosition;
        var differenceFloat = ((Vector2)difference).Normalized();
        var distance = difference.Length();
        var arrowTilesPerSecond = Projectile.TilesPerSecond * BattleTileMap.TileSize;
        var unitTilesPerSecond = 2 * BattleTileMap.TileSize;
        var speed = arrowTilesPerSecond + unitTilesPerSecond;
        var timeToHit = distance / speed;
        var projectedPos = new Vector2((float)(unitTilesPerSecond * timeToHit * differenceFloat.X), (float)(unitTilesPerSecond * timeToHit * differenceFloat.Y));
        var projectedPosTile = Globals.BattleScene.TileMap.GlobalToMap(projectedPos);
        
        if (target.IsMoving) targetLoc += new Vector2I(projectedPosTile.X, 0);
        
        // // If the target is moving, lead the target by guessing they will be closer to the wielder in the future
        // if (target.IsMoving && difference.Length() > 5) targetLoc += tilesToLead;
        
        var type = Name switch
        {
            "Pilum" => "Pilum",
            _ => "Arrow"
        };
        
        Globals.BattleScene.AddChild(new Projectile(wielder.MapPosition, targetLoc, mapPosition => ApplyDamage(wielder, mapPosition), type, target));
        PlayFiredSound(wielder);
    }

    private void ApplyDamage(Unit wielder, Vector2I mapPosition)
    {
        var target = Globals.BattleScene.TileMap.GetEntity(mapPosition, BattleTileMap.Unit) as Unit;
        if (target == null) return;
        
        var damage = Damage + Mathf.FloorToInt(wielder.Strength / 3.0f);
        
        var armor = target.Armor.Sum(armor => armor.Protection);
        var armorRoll = Utils.RollDice("2d6");
        var damageRoll = Utils.RollDice("2d6");

        var adjustedDamage = damage - armor + damageRoll - armorRoll;
        adjustedDamage = Mathf.Max(adjustedDamage, 0);
        
        target.Damage(wielder, adjustedDamage);
        if (adjustedDamage <= 0) return;
        
        
        PlayHitSound(target);
        
    }

    private void PlayHitSound(Unit target)
    {
        var randomizer = new AudioStreamRandomizer();
        randomizer.AddStream(0,
            GD.Load<AudioStream>(
                "res://res/sfx/zapsplat_warfare_arrow_incoming_whoosh_hit_body_squelch_blood_010_90731.mp3"));
        target.Audio.Stream = randomizer;
        target.Audio.VolumeDb = -20;
        target.Audio.Play();
    }

    private void PlayFiredSound(Unit wielder)
    {
        var randomizer = new AudioStreamRandomizer();
        randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/warfare_medieval_english_longbow_arrow_shoot_from_bow_001.mp3"));
        randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/warfare_medieval_english_longbow_arrow_shoot_from_bow_002.mp3"));
        wielder.Audio.Stream = randomizer;
        wielder.Audio.VolumeDb = -20;
        wielder.Audio.Play();
    }
}