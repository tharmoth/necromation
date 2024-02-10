﻿using Godot;

namespace Necromation;

public abstract partial class Interactable  : Node2D
{
    
    private Tween _tween;
    
    [Export]
    private float duration = 1.0f;

    public void Interact()
    {
        if (!CanInteract() || _tween != null) return;
        
        var progressBar = GetNode<ProgressBar>("ProgressBar");
        progressBar.Value = 0;
        progressBar.Visible = true;

        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(progressBar, "value", 100, duration);
        _tween.TweenCallback(new Callable(this, "Complete"));
    }

    protected virtual void Complete()
    {
        _tween = null;
        var progressBar = GetNode<ProgressBar>("ProgressBar");
        progressBar.Value = 0;
        progressBar.Visible = false;
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
}