﻿namespace Necromation.gui;

public interface ICrafter
{
    /*
     * Return the recipe that the crafter is currently using
     */
    public Recipe GetRecipe(Recipe recipe);
    
    /*
     * Set the recipe that the crafter should use
     */
    public void SetRecipe(Recipe recipe);
    
    /*
     * Returns the inventory that the crafter uses to get the ingredients
     */
    public Inventory GetInputInventory();
    
    /*
     * Returns the inventory that the crafter uses to store the products
     */
    public Inventory GetOutputInventory();
}