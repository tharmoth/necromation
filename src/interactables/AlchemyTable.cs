using System.Collections.Generic;
using Godot;
using Necromation.gui;

namespace Necromation;

public partial class AlchemyTable : Interactable
{
    private readonly Recipe _recipe = new Recipe("Potion", new Dictionary<string, int>() { {"Nightshade", 3}});
    
    protected override void Complete()
    {
        base.Complete();
        if (_recipe.CanCraft(Inventory.Instance))
        {
            _recipe.Craft(Inventory.Instance);
        }
        else
        {
            GD.Print("You do not have enough bones or nightshade to brew a potion.");
        }
    }
    
    public override bool CanInteract()
    {
        return _recipe.CanCraft(Inventory.Instance) && base.CanInteract();
    }
}