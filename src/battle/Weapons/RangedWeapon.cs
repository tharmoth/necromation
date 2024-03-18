﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.sk;

namespace Necromation.map.battle.Weapons;

public class RangedWeapon : Weapon
{
    public RangedWeapon(string name, int range, int damage, int hands, int cooldown, int ammo) : base(name, range, damage, hands, cooldown, ammo)
    {

    }
    
    protected override void Attack(Unit wielder, Unit target)
    {
        wielder.PlayAttackAnimation();
        var targetLocs = Globals.BattleScene.TileMap.GetTilesInRadius(target.MapPosition, 2).ToList();
        var targetLoc = targetLocs[GD.RandRange(0, targetLocs.Count - 1)];

        var difference = wielder.MapPosition - target.MapPosition;
        var differenceFloat = ((Vector2)difference).Normalized();
        var tilesToLead = new Vector2I(Mathf.CeilToInt(Mathf.Abs(differenceFloat.X)) * Mathf.Sign(differenceFloat.X), Mathf.CeilToInt(Mathf.Abs(differenceFloat.Y)) * Mathf.Sign(differenceFloat.Y));

        // If the target is moving, lead the target by guessing they will be closer to the wielder in the future
        if (target.IsMoving && difference.Length() > 5) targetLoc += tilesToLead;
        
        var type = Name switch
        {
            "Pilum" => "Pilum",
            _ => "Arrow"
        };
        
        Globals.BattleScene.AddChild(new Arrow(wielder.MapPosition, targetLoc, hit => ApplyDamage(wielder, hit), type));
        PlayFiredSound(wielder);
    }
    
    private void ApplyDamage(Unit wielder, Unit target)
    {
        var damage = Damage + Mathf.FloorToInt(wielder.Strength / 3.0f);
        
        var armor = target.Armor.Sum(armor => armor.Protection);
        var armorRoll = Utils.RollDice("2d6");
        var damageRoll = Utils.RollDice("2d6");

        var adjustedDamage = damage - armor + damageRoll - armorRoll;
        
        if (adjustedDamage <= 0) return;
        
        target.Damage(wielder, adjustedDamage);
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