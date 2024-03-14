using Godot;
using Necromation.interfaces;
using Necromation.sk;

namespace Necromation.factory.character.Actions;

public class InteractAction
{
    private readonly Inventory _inventory;
    
    public InteractAction(Inventory inventory)
    {
        _inventory = inventory;
    }
    
    public bool ShouldInteract(LayerTileMap.IEntity building)
    {
        return Input.IsActionJustPressed("left_click") 
               && !Building.IsBuilding(Globals.Player.Selected)
               && !Input.IsKeyPressed(Key.Ctrl) 
               && building is IInteractable interactable;
    }
    
    public void Interact(LayerTileMap.IEntity building)
    {
        if (building is not IInteractable interactable) return;
        Globals.Player.Selected = null;
        interactable.Interact(_inventory);
    }
}