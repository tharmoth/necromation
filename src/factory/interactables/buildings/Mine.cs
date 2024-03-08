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
    private GpuParticles2D _particles = GD.Load<PackedScene>("res://src/factory/interactables/buildings/drilling.tscn")
        .Instantiate<GpuParticles2D>();

    public override string ItemType => "Mine";
    private Resource _resource;
    private Tween tweenytwiney;
    
    public Mine() 
    {
        _audio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_transport_bicycle_ride_gravel_onboard_pov_10530.mp3");
        _audio.Attenuation = 15.0f;
        _audio.Autoplay = true;
        _audio.VolumeDb = -20.0f;
        _audio.PitchScale = .5f;
        _audio.Finished += () => _audio.Play();
        Sprite.AddChild(_audio);
        Sprite.AddChild(_particles);
    }

    public override void _Ready()
    {
        base._Ready();
        _audio.CallDeferred("play", (float)GD.RandRange(0.0f, 34.0f));
        _resource = Globals.FactoryScene.TileMap.GetEntityPositions(this)
            .Select(position => Globals.FactoryScene.TileMap.GetEntity(position, FactoryTileMap.Resource))
            .OfType<Resource>()
            .FirstOrDefault();
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


        _time += (float)delta;
        Animate();

        if (GetProgressPercent() < 1.0f) return;
        _time = 0;

        _inventory.Insert(_resource.ItemType);
        ShowText(_resource);
    }
    
    public override float GetProgressPercent()
    {
        return _time / _miningSpeed;
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) && GetOccupiedPositions(position).Any(Globals.FactoryScene.TileMap.IsResource);
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private bool MaxOutputItemsReached()
    {
        return _inventory.CountAllItems() >= 200;
    }
    
    private void Animate()
    {
        if (!Config.AnimateMines && _particles.Emitting) _particles.Emitting = false; 
        if (!_particles.Emitting && Config.AnimateMines) _particles.Emitting = true;
        if (!IsOnScreen || tweenytwiney != null && tweenytwiney.IsRunning() || !Config.AnimateMines) return;
        
        // Get random position
        var randomPosition = new Vector2((float)GD.RandRange(-2.0f, 2.0f), (float)GD.RandRange(-3.0f, 0f));
        tweenytwiney?.Kill();
        tweenytwiney = Globals.Tree.CreateTween();
        tweenytwiney.TweenProperty(Sprite, "global_position", GlobalPosition + GetSpriteOffset() + randomPosition, .1f);
        tweenytwiney.TweenProperty(Sprite, "global_position", GlobalPosition + GetSpriteOffset(), .1f);
        tweenytwiney.TweenCallback(Callable.From(() => tweenytwiney.Kill()));
    }

    private void ShowText(Resource collectable)
    {
        if (!IsOnScreen) return;
        Label text = new();
        text.AutowrapMode = TextServer.AutowrapMode.Off;
        text.CustomMinimumSize = new Vector2(1000, 100);
        
        text.Text = collectable.ItemType + " +1";
        text.GlobalPosition = GlobalPosition - GetSpriteOffset();
        text.ZIndex = 100;
        text.Set("theme_override_font_sizes/font_size", 7);
        
        Globals.FactoryScene.AddChild(text);

        var textPositionTween = Globals.Tree.CreateTween();
        textPositionTween.TweenProperty(text, "global_position", text.GlobalPosition + new Vector2(0, -50), 1.0f);
        textPositionTween.TweenCallback(Callable.From(() => text.QueueFree()));
        
        var textColorTween = Globals.Tree.CreateTween();
        textColorTween.TweenProperty(text, "modulate", new Color(1, 1, 1, 0), 1.0f);
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
        Globals.FactoryScene.Gui.Display(playerInventory, _inventory, ItemType);
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