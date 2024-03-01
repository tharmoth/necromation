﻿using Godot;

namespace Necromation;

public class MusicManager
{
    
    private static AudioStreamPlayer _ambiancePlayer;
    public static AudioStreamPlayer AmbiancePlayer
    {
        get
        {
            if (_ambiancePlayer == null)
            {
                _ambiancePlayer = new AudioStreamPlayer();
                _ambiancePlayer.VolumeDb = -30;
                // _ambiancePlayer.Finished += () => _ambiancePlayer.Play();
                Globals.Tree.Root.CallDeferred("add_child", _ambiancePlayer);
            }
            return _ambiancePlayer;
        }
    }
        
    private static AudioStreamPlayer _musicPlayer;
    public static AudioStreamPlayer MusicPlayer
    {
        get
        {
            if (_musicPlayer == null)
            {
                _musicPlayer = new AudioStreamPlayer();
                _musicPlayer.VolumeDb = -30;
                // _musicPlayer.Finished += () => _musicPlayer.Play();
                Globals.Tree.Root.CallDeferred("add_child", _musicPlayer);
            }
            return _musicPlayer;
        }
    }
    
    private static AudioStreamPlayer _sfxPlayer;
    public static AudioStreamPlayer SfxPlayer
    {
        get
        {
            if (_sfxPlayer == null)
            {
                _sfxPlayer = new AudioStreamPlayer();
                _sfxPlayer.VolumeDb = -30;
                // _musicPlayer.Finished += () => _musicPlayer.Play();
                Globals.Tree.Root.CallDeferred("add_child", _sfxPlayer);
            }
            return _sfxPlayer;
        }
    }

    private static string _currentMusic = "";
    private static bool _explored = false;
    public static void PlayBattleMusic()
    {
        if (_currentMusic == "battle") return;
        _currentMusic = "battle";
        MusicPlayer.Stream = GD.Load<AudioStream>("res://res/sfx/music/Fantasy Suspense Main.wav");
        MusicPlayer.CallDeferred("play");
        AmbiancePlayer.Stop();
    }
    
    public static void PlayExploration()
    {
        if (_currentMusic == "exploration" || _explored) return;
        _currentMusic = "exploration";
        MusicPlayer.Stream = GD.Load<AudioStream>("res://res/sfx/music/Fantasy Exploration Main.wav");
        MusicPlayer.CallDeferred("play");
        _explored = true;
    }
    
    public static void PlayAmbiance()
    {
        AmbiancePlayer.Stream = GD.Load<AudioStream>("res://res/sfx/ambiance/forest - wind, birds.wav");
        AmbiancePlayer.CallDeferred("play");
    }

    public static void PlayCraft()
    {
        var stream = new AudioStreamRandomizer();
        stream.AddStream(0, GD.Load<AudioStream>("res://res/sfx/crafting/zapsplat_foley_wet_sand_hand_dig_in_004_41512.mp3"));
        stream.AddStream(0, GD.Load<AudioStream>("res://res/sfx/crafting/zapsplat_foley_wet_sand_hand_dig_in_005_41513.mp3"));
        stream.AddStream(0, GD.Load<AudioStream>("res://res/sfx/crafting/zapsplat_foley_wet_sand_hand_dig_in_006_41514.mp3"));
        SfxPlayer.Stream = stream;
        SfxPlayer.VolumeDb = -20;
        SfxPlayer.PitchScale = .8f;
        SfxPlayer.CallDeferred("play");
    }
}