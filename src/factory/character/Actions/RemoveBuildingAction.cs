using Godot;
using Necromation.sk;

namespace Necromation.factory.character.Actions;

public partial class RemoveBuildingAction : GodotObject
{
    private readonly Inventory  _inventory;
    private Building _buildingBeingRemoved;
    private Tween _removeTween;
    
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
        _removeTween?.Kill();
        _removeTween = Globals.Tree.CreateTween();
        _removeTween.TweenProperty(this, "RemovePercent", 1.0f, .333f);
        _removeTween.TweenCallback(Callable.From(() =>
        {
            RemovePercent = 100;
            _buildingBeingRemoved.Remove(_inventory);
        }));
    }

    public void CancelRemoval()
    {
        RemovePercent = 0;
        _removeTween?.Kill();
        _removeTween = null;
        _buildingBeingRemoved = null;
    }
    
    public bool IsRemoving => _buildingBeingRemoved != null;
}