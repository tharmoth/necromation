namespace Necromation.gui;

public interface ICrafter
{
    public Recipe GetRecipe(Recipe recipe);
    public void SetRecipe(Recipe recipe);
    public Inventory GetInputInventory();
    public Inventory GetOutputInventory();
}