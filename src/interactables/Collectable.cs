using Godot;
using Necromation.sk;

namespace Necromation;

public partial class Collectable : Node2D, LayerTileMap.IEntity
{
    [Export] public string Type { get; set; } = "Stone";
    [Export] private float _duration = 1.0f;
    public string ItemType => Type;

    private Tween _tween;
        
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("resources");
        GetNode<Sprite2D>("Sprite2D").Texture = GD.Load<Texture2D>($"res://res/sprites/{Type}.png");
    }
    
    public void Interact(Inventory playerInventory)
    {
        if (!CanInteract() || _tween != null) return;
        
        var progressBar = GetNode<ProgressBar>("ProgressBar");
        progressBar.Value = 0;
        progressBar.Visible = true;

        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(progressBar, "value", 100, _duration);
        _tween.TweenCallback(Callable.From(() => Complete(playerInventory)));
    }

    protected void Complete(Inventory playerInventory)
    {
        _tween = null;
        var progressBar = GetNode<ProgressBar>("ProgressBar");
        progressBar.Value = 0;
        progressBar.Visible = false;
        playerInventory.Insert(Type);
    }

    public virtual bool CanInteract()
    {
        return _tween == null;
    }

    public bool Interacting()
    {
        return _tween != null;
    }
    
    public void Cancel()
    {
        _tween?.Kill();
        _tween = null;
        var progressBar = GetNode<ProgressBar>("ProgressBar");
        progressBar.Value = 0;
        progressBar.Visible = false;
    }

    public bool CanRemove()
    {
        return false;
    }
}