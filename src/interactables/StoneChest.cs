using Necromation.interfaces;

namespace Necromation;

public partial class StoneChest : Building, Inserter.ITransferTarget, IInteractable
{
    public override string ItemType => "Stone Chest";
    Inventory _inventory = new();

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

    public bool CanAcceptItem(string item)
    {
        // If the chest already has 200 of that type of item, don't accept it
        return _inventory.CountItem(item) < 200;
    }

    public void Interact()
    {
        GUI.Instance.ContainerGui.Display(Globals.PlayerInventory, _inventory, ItemType);
    }
}