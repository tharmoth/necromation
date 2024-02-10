using Necromation.interfaces;

namespace Necromation;

public partial class StoneChest : Building, Inserter.ITransferTarget, IInteractable
{
    Inventory _inventory = new();
    
    public override string ItemType => "Stone Chest";
    public override bool CanRemove()
    {
        return true;
    }

    public override float GetProgressPercent()
    {
        return 0;
    }

    public Inventory GetInputInventory()
    {
        return _inventory;
    }

    public Inventory GetOutputInventory()
    {
        return _inventory;
    }
    
    public void Interact()
    {
        GUI.Instance.ContainerGui.Display(Globals.PlayerInventory, _inventory, ItemType);
    }
}