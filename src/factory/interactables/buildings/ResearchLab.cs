using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class ResearchLab : Building, ITransferTarget, IInteractable
{
    /**************************************************************************
     * Building Implementation                                                *
     **************************************************************************/
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Research Lab";
    
    /**************************************************************************
     * Hardcoded Scene Imports 											      *
     **************************************************************************/
    public static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/soul_storm.tscn");
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    private ResearchLabInventory _inventory = new();
    private bool _isResearching;
    private double _researchedAmount = 0;
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private Tween _animationTween;
    private readonly GpuParticles2D _particles;
    
    public ResearchLab()
    {
        _particles = Scene.Instantiate<GpuParticles2D>();
        Sprite.AddChild(_particles);
    }

    public override void _Process(double delta)
    {
        if (_isResearching && !_particles.Emitting) _particles.Emitting = true;
        else if (!_isResearching && _particles.Emitting) _particles.Emitting = false;
        
        if (Globals.CurrentTechnology == null)
        {
            _isResearching = false;
            return;
        }

        if (_isResearching)
        {
            _researchedAmount += delta / 30;
            Globals.CurrentTechnology.Progress += delta / 30;
            Animate();
            if (_researchedAmount < 1.0f) return;
            _isResearching = false;
            _researchedAmount = 0;
            return;
        }

        // if (!GetItems().Contains(Globals.CurrentTechnology.Ingredients[0])) return;
        // Remove(Globals.CurrentTechnology.Ingredients[0]);

        if (Globals.Souls <= 0) return;
        Globals.Souls--;
        
        _isResearching = true;
    }
    
    protected void Animate()
    {
        if (GodotObject.IsInstanceValid(_animationTween) && _animationTween.IsRunning()) return;

        _animationTween?.Kill();
        _animationTween = Globals.Tree.CreateTween();
        _animationTween.TweenProperty(Sprite, "scale", new Vector2(.85f, .85f), 1f);
        _animationTween.TweenProperty(Sprite, "scale", Vector2.One, 1f);
        _animationTween.TweenCallback(Callable.From(() => _animationTween.Kill()));
    }

    #region IInteractable Implementation
    /**************************************************************************
     * IInteractable Methods                                                  *
     **************************************************************************/        
    public void Interact(Inventory playerInventory)
    {
        ContainerGui.Display(playerInventory, _inventory, ItemType);;
    }
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    private class ResearchLabInventory : Inventory
    {
        public override int GetMaxTransferAmount(string itemType)
        {
            if (!itemType.Contains("Experiment")) return 0;

            var currentCount = CountItem(itemType);
            return Mathf.Max(0,  10 - currentCount);
        }
    }
    
    public bool CanAcceptItems(string item, int count = 1) => _inventory.CanAcceptItems(item, count);
    public bool CanAcceptItemsInserter(string item, int count = 1) => _inventory.CanAcceptItemsInserter(item, count);
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion
}