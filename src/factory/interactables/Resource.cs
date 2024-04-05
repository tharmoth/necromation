using Godot;
using Necromation.sk;

namespace Necromation;

public partial class Resource : Node2D, LayerTileMap.IEntity
{
    [Export] public string Type { get; set; } = "Stone";
    [Export] private float _duration = .33f;
    
    // The time in seconds between audio plays.
    private double _timeBetweenAudio = 0.33;
    // Timer used to play audio at a pleasing frequency.
    private double _time = 0;
    
    public string ItemType => Type;

    private Tween _progressTween;
    private Tween _jiggleTween;
    private Sprite2D _sprite = new();
    private AudioStreamPlayer2D _audio = new();
    private AudioStreamPlayer2D _bonusAudio = new();
    
    private GpuParticles2D _particles = GD.Load<PackedScene>("res://src/factory/interactables/buildings/drilling.tscn").Instantiate<GpuParticles2D>();
    
    private float _progressPercent;
    public float ProgressPercent
    {
        get => _progressPercent;
        set
        {
            _progressPercent = value;
            Globals.FactoryScene.Gui.SetProgress(value);
        }
    }
    
    public Resource(string itemType)
    {
        Type = itemType;
        _sprite.Texture = Database.Instance.GetTexture(Type);
        _sprite.Scale = (Vector2.One * FactoryTileMap.TileSize * 1.15f) / _sprite.Texture.GetSize();
        _sprite.FlipH = GD.Randf() > .5;
        _sprite.Position += new Vector2(GD.RandRange(-3, 3), GD.RandRange(-3, 3));
        AddChild(_sprite);

        var stream = new AudioStreamRandomizer();
        stream.AddStream(0, GD.Load<AudioStream>("res://res/sfx/zapsplat_industrial_pick_axe_single_hit_on_rock_002_103427.mp3"));
        stream.AddStream(1, GD.Load<AudioStream>("res://res/sfx/zapsplat_industrial_pick_axe_single_hit_on_rock_010_103435.mp3"));
        _audio.Stream = stream;
        _audio.PitchScale = .9f;
        _audio.VolumeDb = -15;
        AddChild(_audio);
        
        _bonusAudio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_foley_dry_dead_leaf_crush_001_70766.mp3");
        AddChild(_bonusAudio);
        
        _particles.Position = Vector2.Zero;
        _particles.Emitting = false;
        AddChild(_particles);

        if (Type == "Stone")
        {
            _duration = 1.0f;
            _timeBetweenAudio = 0.5f;
        }
    }
        
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("resources");
        AddToGroup("Persist");
        Globals.FactoryScene.TileMap.AddEntity(GlobalPosition, this, FactoryTileMap.Resource);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Globals.FactoryScene.TileMap.RemoveEntity(this);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        _time += delta;
        if (!Interacting() || !(_time > _timeBetweenAudio)) return;
        _time = 0;
        _audio.Play();
    }

    public void Interact(Inventory playerInventory)
    {
        if (!CanInteract() || _progressTween != null) return;

        ProgressPercent = 0;

        _progressTween?.Kill();
        _progressTween = CreateTween();
        _progressTween.TweenProperty(this, "ProgressPercent", 1.0f, _duration);
        _progressTween.TweenCallback(Callable.From(() => Complete(playerInventory)));
        
        _jiggleTween?.Kill();
        _jiggleTween = CreateTween();
        _jiggleTween.TweenProperty(_sprite, "rotation_degrees", 10, 0.1f);
        _jiggleTween.TweenProperty(_sprite, "rotation_degrees", -10, 0.1f);
        _jiggleTween.TweenProperty(_sprite, "rotation_degrees", 0, 0.1f);
        
        _particles.Emitting = true;
    }

    protected void Complete(Inventory playerInventory)
    {
        _progressTween = null;
        ProgressPercent = 1.0f;
        _particles.Emitting = false;

        playerInventory.Insert(Type);
        var words = ItemType + " " + Globals.PlayerInventory.CountItem(ItemType);

        if (GD.Randf() > .925)
        {
            playerInventory.Insert(Type, 4);
            words = "[color=green]" + words + "[/color]";
            _bonusAudio.Play();
        }
        
        SKFloatingLabel.Show(words, GlobalPosition);
    }

    public virtual bool CanInteract()
    {
        return _progressTween == null;
    }

    public bool Interacting()
    {
        return _progressTween != null;
    }
    
    public void Cancel()
    {
        _progressTween?.Kill();
        _progressTween = null;
        ProgressPercent = 0;
        _particles.Emitting = false;
    }

    public bool CanRemove()
    {
        return false;
    }
    
    #region Save/Load
    /******************************************************************
     * Save/Load                                                      *
     ******************************************************************/
    public Godot.Collections.Dictionary<string, Variant> Save()
    {
        return new Godot.Collections.Dictionary<string, Variant>()
        {
            { "ItemType", ItemType },
            { "PosX", Position.X }, // Vector2 is not supported by JSON
            { "PosY", Position.Y },
        };
    }
	
    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        Resource resource = new Resource(nodeData["ItemType"].ToString());
        resource.GlobalPosition = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
        Globals.FactoryScene.AddChild(resource);
    }
    #endregion
}