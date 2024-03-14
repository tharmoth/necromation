using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.factory.character.Actions;

public class BuildAction
{
    private readonly Inventory _inventory;
    
    public BuildAction(Inventory inventory)
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

        Globals.BuildingManager.AddBuilding(building, position);

        if(!_inventory.Items.ContainsKey(building.ItemType)) Globals.Player.Selected = null;

        PlayBuildAnimation();
        return true;
    }

    private Tween buildTween;
    
    
    private void PlayBuildAnimation()
    {
        if (buildTween != null) return;
        var sprite = Globals.Player.Hands;
        sprite.Visible = true;
        sprite.Position = Vector2.Zero;

        buildTween = Globals.Tree.CreateTween();
        buildTween.SetEase(Tween.EaseType.In);
        buildTween.SetTrans(Tween.TransitionType.Quart);
        
        buildTween.TweenInterval(.05f);
        buildTween.TweenProperty(sprite, "position:y", -96, .5f);
        buildTween.TweenInterval(.5f);
        buildTween.TweenCallback(Callable.From(() => sprite.Visible = false));
        buildTween.TweenCallback(Callable.From(() => buildTween = null));
        
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