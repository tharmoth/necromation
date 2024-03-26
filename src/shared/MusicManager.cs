using Godot;

namespace Necromation;

public class MusicManager
{
    
    private static AudioStreamPlayer _ambiancePlayer;
    private static AudioStreamPlayer AmbiancePlayer
    {
        get
        {
            if (_ambiancePlayer == null)
            {
                _ambiancePlayer = new AudioStreamPlayer();
                _ambiancePlayer.VolumeDb = -30;
                _ambiancePlayer.Finished += () => _ambiancePlayer.Play();
                Globals.Tree.Root.CallDeferred("add_child", _ambiancePlayer);
            }
            return _ambiancePlayer;
        }
    }
        
    private static AudioStreamPlayer _musicPlayer;
    private static AudioStreamPlayer MusicPlayer
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
    private static AudioStreamPlayer SfxPlayer
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
        if (_currentMusic == "battle" && MusicPlayer.Playing) return;
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

    public static void Play(string sound)
    {
        // This will probably need to be reworked as more sounds are added. perhaps a json?
        AudioStream path = null;
        switch (sound)
        {
            case "research_complete":
                path = GD.Load<AudioStream>("res://res/sfx/jingles-pizzicato_14.ogg");
                SfxPlayer.PitchScale = 1.0f;
                SfxPlayer.VolumeDb = -10;
                break;
            case "ui_open":
                path = new AudioStreamRandomizer();
                ((AudioStreamRandomizer) path).AddStream(0, 
                    GD.Load<AudioStream>("res://res/sfx/kenny-audio/handleSmallLeather.ogg"));
                SfxPlayer.PitchScale = 1.2f;
                SfxPlayer.VolumeDb = 0;
                break;
            case "ui_close":
                path = new AudioStreamRandomizer();
                ((AudioStreamRandomizer) path).AddStream(0, 
                    GD.Load<AudioStream>("res://res/sfx/kenny-audio/handleSmallLeather.ogg"));
                SfxPlayer.PitchScale = .8f;
                SfxPlayer.VolumeDb = 0;
                break;
            case "save":
                path = GD.Load<AudioStream>("res://res/sfx/jingles-pizzicato_14.ogg");
                SfxPlayer.PitchScale = 1.0f;
                SfxPlayer.VolumeDb = -10;
                break;
            case "craft":
                path = new AudioStreamRandomizer();
                ((AudioStreamRandomizer) path).AddStream(0, 
                    GD.Load<AudioStream>("res://res/sfx/crafting/zapsplat_foley_wet_sand_hand_dig_in_004_41512.mp3"));
                ((AudioStreamRandomizer) path).AddStream(0, 
                    GD.Load<AudioStream>("res://res/sfx/crafting/zapsplat_foley_wet_sand_hand_dig_in_005_41513.mp3"));
                ((AudioStreamRandomizer) path).AddStream(0, 
                    GD.Load<AudioStream>("res://res/sfx/crafting/zapsplat_foley_wet_sand_hand_dig_in_006_41514.mp3"));
                SfxPlayer.VolumeDb = -30;
                SfxPlayer.PitchScale = .8f;
                break;
        }

        if (path == null)
        {
            GD.PrintErr($"Sound '{sound}' not found");
            return;
        }
        
        SfxPlayer.Stream = path;
        SfxPlayer.CallDeferred("play");
    }
}