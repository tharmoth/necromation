using System;
using Godot;
using Necromation;
public class FurnaceAnimationComponent
{
    public interface IAnimatable
    {
        public event Action StartAnimation;
        public event Action StopAnimation;
        public Sprite2D Sprite { get; }
    }
    
    /**************************************************************************
     * Hardcoded Scene Imports 											      *
     **************************************************************************/
    private static readonly PackedScene ParticleScene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/smoke.tscn");
    
    /**************************************************************************
     * Private Variables 													  *
     **************************************************************************/
    private readonly GpuParticles2D _particles;
    private readonly PointLight2D _light = new();
    private readonly Sprite2D _sprite;
    private Tween _animationTween;
    
    /**************************************************************************
     * Constructor      													  *
     **************************************************************************/
    public FurnaceAnimationComponent(IAnimatable furnace)
    {
        furnace.StartAnimation += PlayWorkingAnimation;
        furnace.StopAnimation += StopWorkingAnimation;
        _sprite = furnace.Sprite;
        
        _particles = ParticleScene.Instantiate<GpuParticles2D>();
        _particles.Emitting = false;
        _sprite.AddChild(_particles);
	    
        _light.Visible = false;
        _light.Texture = Database.Instance.GetTexture("SoftLight");
        _light.Color = Colors.Yellow;
        _light.TextureScale = .3f;
        _light.Position = new Vector2(0, 24);
        _sprite.AddChild(_light);
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private void PlayWorkingAnimation()
    {
        if (GodotObject.IsInstanceValid(_animationTween) && _animationTween.IsRunning()) return;

        _animationTween?.Kill();
        _animationTween = _sprite.CreateTween();
        _animationTween.TweenProperty(_sprite, "scale", new Vector2(1.0f, .85f), 1f);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One, 1f);
        _animationTween.TweenCallback(Callable.From(() => _animationTween.Kill()));
	    
        _particles.Emitting = true;
        _light.Visible = true;
    }

    private void StopWorkingAnimation()
    {
        _sprite.Scale = Vector2.One;
        _animationTween?.Kill();
        _particles.Emitting = false;
        _light.Visible = false;
    }
}