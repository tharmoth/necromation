using Godot;

namespace Necromation.factory.character.Actions;

public class MineAction
{
    private Resource _resource;
    private readonly Inventory _inventory;
    
    public MineAction(Inventory inventory)
    {
        _inventory = inventory;
    }

    public bool ShouldMine()
    {
        return Input.IsMouseButtonPressed(MouseButton.Right) &&
               Globals.FactoryScene.TileMap.GetBuildingAtMouse() == null;
    }
    
    public void Mine()
    {
        if (Globals.FactoryScene.TileMap.GetResourceAtMouse() is Resource resource)
        {
            if (resource == _resource && !_resource.CanInteract()) return;
            _resource?.Cancel();
            _resource = resource;
            _resource.Interact(_inventory);
        }
        else
        {
            _resource?.Cancel();
            _resource = null;
        }
    }
    
    public void Cancel()
    {
        _resource?.Cancel();
        _resource = null;
    }
}