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
        var building = Building.GetBuilding(Globals.Player.Selected, Globals.Player.Orientation);
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
        return true;
    }
}