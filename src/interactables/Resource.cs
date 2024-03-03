using Godot;
using Necromation.sk;

namespace Necromation;

public partial class Resource : Node2D, LayerTileMap.IEntity
{
    [Export] public string Type { get; set; } = "Stone";
    [Export] private float _duration = .33f;
    public string ItemType => Type;

    private Tween _progressTween;
    private Tween _jiggleTween;
    private Sprite2D _sprite = new();
    private ProgressBar _progressBar = new();
    private AudioStreamPlayer2D _audio = new();
    private AudioStreamPlayer2D _bonusAudio = new();
    
    public Resource(string itemType)
    {
        Type = itemType;
        _sprite.Texture = Globals.Database.GetTexture(Type);
        AddChild(_sprite);
        
        _progressBar.Scale = new Vector2(0.25f, 0.25f);
        _progressBar.Position -= new Vector2(16, -6);
        _progressBar.Size = new Vector2(128, 28);
        _progressBar.ShowPercentage = false;
        _progressBar.Visible = false;
        // AddChild(_progressBar);

        var stream = new AudioStreamRandomizer();
        stream.AddStream(0, GD.Load<AudioStream>("res://res/sfx/zapsplat_industrial_pick_axe_single_hit_on_rock_002_103427.mp3"));
        stream.AddStream(1, GD.Load<AudioStream>("res://res/sfx/zapsplat_industrial_pick_axe_single_hit_on_rock_010_103435.mp3"));
        _audio.Stream = stream;
        _audio.PitchScale = .9f;
        _audio.VolumeDb = -15;
        AddChild(_audio);
        
        _bonusAudio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_foley_dry_dead_leaf_crush_001_70766.mp3");
        AddChild(_bonusAudio);

        if (Type == "Stone") _duration = 1.0f;
    }
        
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("resources");
        AddToGroup("Persist");
        Globals.TileMap.AddEntity(GlobalPosition, this, BuildingTileMap.Resource);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Globals.TileMap.RemoveEntity(this);
    }

    public void Interact(Inventory playerInventory)
    {
        if (!CanInteract() || _progressTween != null) return;
        
        _progressBar.Value = 0;
        _progressBar.Visible = true;

        _progressTween?.Kill();
        _progressTween = CreateTween();
        _progressTween.TweenProperty(_progressBar, "value", 100, _duration);
        _progressTween.TweenCallback(Callable.From(() => Complete(playerInventory)));
        
        _jiggleTween?.Kill();
        _jiggleTween = CreateTween();
        _jiggleTween.TweenProperty(_sprite, "rotation_degrees", 5, 0.1f);
        _jiggleTween.TweenProperty(_sprite, "rotation_degrees", -5, 0.1f);
        _jiggleTween.TweenProperty(_sprite, "rotation_degrees", 0, 0.1f);

        _audio.Play();
    }

    protected void Complete(Inventory playerInventory)
    {
        _progressTween = null;
        _progressBar.Value = 0;
        _progressBar.Visible = false;

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
        _progressBar.Value = 0;
        _progressBar.Visible = false;
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