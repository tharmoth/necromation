using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class Mine : Building, IInteractable, ITransferTarget
{
    public override Vector2I BuildingSize => Vector2I.One * 2;
    private Inventory _inventory = new();
    private float _time;
    private float _miningSpeed = 2.0f;
    private AudioStreamPlayer2D _audio = new();
    private GpuParticles2D _particles = GD.Load<PackedScene>("res://src/interactables/buildings/drilling.tscn")
        .Instantiate<GpuParticles2D>();

    public override string ItemType => "Mine";
    
    public Mine() 
    {
        _audio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_transport_bicycle_ride_gravel_onboard_pov_10530.mp3");
        _audio.Attenuation = 15.0f;
        _audio.Autoplay = true;
        _audio.VolumeDb = -20.0f;
        _audio.PitchScale = .5f;
        _audio.Finished += () => _audio.Play();
        AddChild(_audio);
        Sprite.AddChild(_particles);
    }

    public override void _Ready()
    {
        base._Ready();
        _audio.Playing = true;
        _audio.Play(fromPosition:(float)GD.RandRange(0.0f, 34.0f));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (MaxOutputItemsReached())
        {
            if (_audio.Playing) _audio.Stop();
            _time = 0;
            if (_particles.Emitting) _particles.Emitting = false;
            return;
        }

        if (!_particles.Emitting) _particles.Emitting = true;
        _time += (float)delta;
        Animate();

        if (GetProgressPercent() < 1.0f) return;
        _time = 0;

        var resource = Globals.TileMap.GetEntityPositions(this)
            .Select(position => Globals.TileMap.GetEntity(position, BuildingTileMap.Resource))
            .FirstOrDefault(resource => resource is Resource);
        
        if (resource is not Resource collectable) return;
        _inventory.Insert(collectable.ItemType);
        ShowText(collectable);
    }
    
    public override float GetProgressPercent()
    {
        return _time / _miningSpeed;
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) && GetOccupiedPositions(position).Any(Globals.TileMap.IsResource);
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private bool MaxOutputItemsReached()
    {
        return _inventory.CountAllItems() >= 200;
    }

    private Tween tweenytwiney;
    
    private void Animate()
    {
        if (IsInstanceValid(tweenytwiney) && tweenytwiney.IsRunning() || IsOnScreen) return;
        
        // Get random position
        var randomPosition = new Vector2((float)GD.RandRange(-2.0f, 2.0f), (float)GD.RandRange(-3.0f, 0f));
        tweenytwiney?.Kill();
        tweenytwiney = GetTree().CreateTween();
        tweenytwiney.TweenProperty(Sprite, "position", GetSpriteOffset() + randomPosition, .1f);
        tweenytwiney.TweenProperty(Sprite, "position", GetSpriteOffset(), .1f);
        tweenytwiney.TweenCallback(Callable.From(() => tweenytwiney.Kill()));
    }

    private void ShowText(Resource collectable)
    {
        if (IsOnScreen) return;
        RichTextLabel text = new();
        text.AutowrapMode = TextServer.AutowrapMode.Off;
        text.CustomMinimumSize = new Vector2(1000, 100);
        
        text.Text = "[font_size=7]" + collectable.ItemType + " +1" + "[/font_size]";
        text.GlobalPosition = GlobalPosition - GetSpriteOffset();
        text.BbcodeEnabled = true;
        text.ZIndex = 100;
        
        Globals.FactoryScene.AddChild(text);

        var textPositionTween = GetTree().CreateTween();
        textPositionTween.TweenProperty(text, "global_position", text.GlobalPosition + new Vector2(0, -50), 1.0f);
        textPositionTween.TweenCallback(Callable.From(() => text.QueueFree()));
        
        var textColorTween = GetTree().CreateTween();
        textColorTween.TweenProperty(text, "modulate", new Color(1, 1, 1, 0), 1.0f);
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
        FactoryGUI.Instance.Display(playerInventory, _inventory, ItemType);
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => false;
    public void Insert(string item, int count = 1) { }
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
}