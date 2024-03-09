using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public class Mine : Building, IInteractable, ITransferTarget
{
    /**************************************************************************
     * Building Implementation                                                *
     **************************************************************************/
    public override string ItemType => "Mine";
    public override Vector2I BuildingSize => Vector2I.One * 2;
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    private float _time;
    private readonly Inventory _inventory;
    private Resource _resource;

    /**************************************************************************
     * FX Variables                                                           *
     **************************************************************************/
    private static readonly PackedScene ParticlesScene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/drilling.tscn");
    private readonly GpuParticles2D _particles = ParticlesScene.Instantiate<GpuParticles2D>();
    private static readonly AudioStream Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_transport_bicycle_ride_gravel_onboard_pov_10530.mp3");
    private readonly AudioStreamPlayer2D _audio = new();
    private Tween _animationTween;
    
    /**************************************************************************
     * Data Constants                                                         *
     **************************************************************************/
    private const float MiningSpeed = 2.0f;
    private const int MaxInputItems = 200;

    private static readonly ImmutableList<string> ValidItems =
        ImmutableList.Create("Stone Ore", "Bone Fragments", "Copper Ore", "Coal Ore", "Tin Ore");
    
    public Mine()
    {
        _inventory = new MineInventory();
        
        _audio.Stream = Stream;
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
        if (GetMaxTransferAmount(_resource.ItemType) == 0)
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
        return _time / MiningSpeed;
    }
    
    // Mines can only be placed over resources.
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) && GetOccupiedPositions(position).Any(Globals.FactoryScene.TileMap.IsResource);
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private bool MaxOutputItemsReached()
    {
        return _inventory.CountAllItems() >= MaxInputItems;
    }
    
    private void Animate()
    {
        if (!Config.AnimateMines && _particles.Emitting) _particles.Emitting = false; 
        if (!_particles.Emitting && Config.AnimateMines) _particles.Emitting = true;
        if (!IsOnScreen || _animationTween != null && _animationTween.IsRunning() || !Config.AnimateMines) return;
        
        // Get random position
        var randomPosition = new Vector2((float)GD.RandRange(-2.0f, 2.0f), (float)GD.RandRange(-3.0f, 0f));
        _animationTween?.Kill();
        _animationTween = Globals.Tree.CreateTween();
        _animationTween.TweenProperty(Sprite, "global_position", GlobalPosition + GetSpriteOffset() + randomPosition, .1f);
        _animationTween.TweenProperty(Sprite, "global_position", GlobalPosition + GetSpriteOffset(), .1f);
        _animationTween.TweenCallback(Callable.From(() => _animationTween.Kill()));
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
        MineGui.Display(playerInventory, _inventory, this, ItemType);;
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    private class MineInventory : Inventory
    {
        public override int GetMaxTransferAmount(string itemType)
        {
            if (!ValidItems.Contains(itemType)) return 0;
            var currentCount = CountAllItems();
            return Mathf.Max(0,  MaxInputItems - currentCount);
        }
    }
    
    public bool CanAcceptItems(string item, int count = 1) => false;
    public void Insert(string item, int count = 1) { }
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    public int GetMaxTransferAmount(string itemType) => _inventory.GetMaxTransferAmount(itemType);
    #endregion
}