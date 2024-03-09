namespace Necromation.gui;

public interface ICrafter
{
    /*
     * Return the recipe that the crafter is currently using
     */
    public Recipe GetRecipe();
    
    /*
     * Set the recipe that the crafter should use
     */
    public void SetRecipe(Inventory dumpInventory, Recipe recipe);
    
    /*
     * Returns the inventory that the crafter uses to get the ingredients
     */
    public Inventory GetInputInventory();
    
    /*
     * Returns the inventory that the crafter uses to store the products
     */
    public Inventory GetOutputInventory();

    /*
     * Returns the item category that the crafter can craft.
     */
    public string GetCategory();

    public string ItemType { get; }
}