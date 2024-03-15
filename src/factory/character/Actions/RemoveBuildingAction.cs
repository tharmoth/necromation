using Godot;
using Necromation.sk;

namespace Necromation.factory.character.Actions;

public partial class RemoveBuildingAction : Node
{
    public bool IsRemoving => _buildingBeingRemoved != null;
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    private readonly Inventory  _inventory;
    private Building _buildingBeingRemoved;
    
    private Tween _removeTimerTween;
    private float _removePercent;
    public float RemovePercent
    {
        get => _removePercent;
        set
        {
            _removePercent = value;
            Globals.FactoryScene.Gui.SetProgress(value);
        }
    }
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private bool _mouseReleased = true;
    private Tween _removeAnimationTween;
    
    
    public RemoveBuildingAction(Inventory inventory)
    {
        _inventory = inventory;
    }

    public bool ShouldRemove()
    {
        return Input.IsMouseButtonPressed(MouseButton.Right);
    }
    
    public void RemoveBuilding(LayerTileMap.IEntity entity)
    {
        if (entity != _buildingBeingRemoved) CancelRemoval();
        if (entity == _buildingBeingRemoved) return;
        if (entity is not Building building) return;
        _buildingBeingRemoved = building;
        _removeTimerTween?.Kill();
        _removeTimerTween = Globals.Tree.CreateTween();
        _removeTimerTween.TweenProperty(this, "RemovePercent", 1.0f, .333f);
        _removeTimerTween.TweenCallback(Callable.From(() =>
        {
            RemovePercent = 100;
            _buildingBeingRemoved.Remove(_inventory);
            PlayRemoveAnimation();
        }));
    }

    public void CancelRemoval()
    {
        RemovePercent = 0;
        _removeTimerTween?.Kill();
        _removeTimerTween = null;
        _buildingBeingRemoved = null;
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Right }) _mouseReleased = true;
    }

    private void PlayRemoveAnimation()
    {
        if (_removeAnimationTween != null || !_mouseReleased) return;
        _mouseReleased = false;
        var sprite = Globals.Player.Hands;
        sprite.Visible = true;
        sprite.Position = new Vector2(0, -96);

        _removeAnimationTween = Globals.Tree.CreateTween();
        _removeAnimationTween.SetEase(Tween.EaseType.In);
        _removeAnimationTween.SetTrans(Tween.TransitionType.Quart);
        
        _removeAnimationTween.TweenInterval(.05f);
        _removeAnimationTween.TweenProperty(sprite, "position:y", 0, .5f);
        _removeAnimationTween.TweenInterval(.5f);
        _removeAnimationTween.TweenCallback(Callable.From(() => sprite.Visible = false));
        _removeAnimationTween.TweenCallback(Callable.From(() => _removeAnimationTween = null));
        
        var jiggletween = Globals.Tree.CreateTween();
        jiggletween.TweenInterval(.25f);
        
        var tweenTime = 1.0f;
        var jiggleCount = 5;
        for (int i = 0; i < 2; i++)
        {
            jiggletween.TweenProperty(sprite, "position:x", 
                GD.RandRange(-12, 12), .1);
        }
		
        jiggletween.TweenProperty(sprite, "position:x", 0, .1);
    }
}