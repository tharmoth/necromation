namespace Necromation;

public interface ITransferTarget
{
    /*
         * Returns the inventory that the crafter uses to get the ingredients
         */
    public Inventory GetInputInventory();
    
    /*
         * Returns the inventory that the crafter uses to store the products
         */
    public Inventory GetOutputInventory();

    public bool CanAcceptItem(string item);
}