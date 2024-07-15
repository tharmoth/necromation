using Godot;

namespace Necromation;

/// <summary>
/// Manages the objects in the global scope.
/// There are several different ways to go about doing this.
///  1) Global Variables
///  2) Singletons (static instance variables) <see cref="Database"/>
///  3) Service Locator
///
/// The service locator pattern <a href="https://gameprogrammingpatterns.com/service-locator.html">Service Locator Pattern</a>
/// </summary>
public class Locator
{
    
    #region BuildingSystem
    public static IBuildingSystem BuildingSystem
    {
        get => _buildingSystem;
        set => _buildingSystem = value ?? new NullBuildingSystem();
    }
    private static IBuildingSystem _buildingSystem = new NullBuildingSystem();
    private class NullBuildingSystem : IBuildingSystem
    {
        public void AddBuilding(Building building, Vector2 globalPosition)
        {
            GD.PrintErr("Locator::BuildingSystem::AddBuilding BuildingSystem could not be located!");
        }

        public void RemoveBuilding(Building building)
        {
            GD.PrintErr("Locator::BuildingSystem::RemoveBuilding BuildingSystem could not be located!");
        }
        
        public void Clear()
        {
            GD.PrintErr("Locator::BuildingSystem::Clear BuildingSystem could not be located!");
        }
        
        public Godot.Collections.Dictionary<string, Variant> Save()
        {
            GD.PrintErr("Locator::BuildingSystem::Save BuildingSystem could not be located!");
            return new Godot.Collections.Dictionary<string, Variant>();
        }
    }
    #endregion
}