using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.factory.character.Actions;

public partial class BuildBuildingAction : Node
{
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    private readonly Inventory _inventory;
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private Tween _buildAnimationTween;
    private bool _mouseReleased = true;
    
    public BuildBuildingAction(Inventory inventory)
    {
        _inventory = inventory;
    }
    
    public bool ShouldBuild()
    {
        return Input.IsMouseButtonPressed(MouseButton.Left) && Building.IsBuilding(Globals.Player.Selected);
    }
    
    public bool Build()
    {
        var building = Building.GetBuilding(Globals.Player.Selected, Globals.Player.SelectionOrientation);
        if (!_inventory.Items.ContainsKey(building.ItemType))
        {
            Globals.Player.Selected = null;
            return false;
        }
		
        var position = Globals.Player.GetGlobalMousePosition();
        if (!building.CanPlaceAt(position)) return false;

        if (building is IRotatable)
        {
            // Remove any buildings that are in the way. This should probably only happen for IRotatable buildings.
            building.GetOccupiedPositions(position)
                .Select(pos => Globals.FactoryScene.TileMap.GetEntity(pos, FactoryTileMap.Building))
                .Select(entity => entity as Building)
                .Where(entity => entity != null)
                .Distinct()
                .ToList()
                .ForEach(bldg => bldg.Remove(_inventory));
        }
		
        _inventory.Remove(building.ItemType);

        Locator.BuildingSystem.AddBuilding(building, position);

        if(!_inventory.Items.ContainsKey(building.ItemType)) Globals.Player.Selected = null;

        PlayBuildAnimation();
        return true;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left }) _mouseReleased = true;
    }

    private void PlayBuildAnimation()
    {
        if (_buildAnimationTween != null || !_mouseReleased) return;
        _mouseReleased = false;
        var sprite = Globals.Player.Hands;
        sprite.Visible = true;
        sprite.Position = Vector2.Zero;

        _buildAnimationTween = Globals.Tree.CreateTween();
        _buildAnimationTween.SetEase(Tween.EaseType.In);
        _buildAnimationTween.SetTrans(Tween.TransitionType.Quart);
        
        _buildAnimationTween.TweenInterval(.05f);
        _buildAnimationTween.TweenProperty(sprite, "position:y", -96, .5f);
        _buildAnimationTween.TweenInterval(.5f);
        _buildAnimationTween.TweenCallback(Callable.From(() => sprite.Visible = false));
        _buildAnimationTween.TweenCallback(Callable.From(() => _buildAnimationTween = null));
        
        var jiggletween = Globals.Tree.CreateTween();
        jiggletween.TweenInterval(.25f);
        for (int i = 0; i < 2; i++)
        {
            jiggletween.TweenProperty(sprite, "position:x", 
                GD.RandRange(-12, 12), .1);
        }
		
        jiggletween.TweenProperty(sprite, "position:x", 0, .1);
    }
}