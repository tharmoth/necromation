using System.Collections.Generic;
using Godot;
using Necromation.gui;

namespace Necromation;

public partial class Alter : Interactable
{
    
    private readonly Recipe _recipe = new Recipe("Zombie", new Dictionary<string, int>() { {"Bones", 1}, {"Potion", 1}});

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("alters");
    }
    
    protected override void Complete()
    {
        base.Complete();
        if (CanInteract())
        {
            _recipe.Craft(Inventory.Instance);
        }
        else
        {
            GD.Print("You do not have enough bones or nightshade to complete the ritual.");
        }

    }
    
    public override bool CanInteract()
    {
        return base.CanInteract() && _recipe.CanCraft(Inventory.Instance);
    }
}