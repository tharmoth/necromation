using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.gui;
using Necromation.interfaces;

public partial class Furnace : Building, ITransferTarget, ICrafter, IInteractable
{
	public override Vector2I BuildingSize => Vector2I.One * 2;
	public override string ItemType => "Stone Furnace";
    private Recipe _recipe;
    private Inventory _inputInventory = new();
    private Inventory _outputInventory = new();
    private float _time;

    public override void _Process(double delta)
    {
    	base._Process(delta);
	    
    	if (_recipe == null || !_recipe.CanCraft(_inputInventory) || MaxOutputItemsReached())
    	{
    		_time = 0;
		    _recipe = Globals.Database.Recipes
			    .Where(recipe => recipe.Category == GetCategory())
			    .FirstOrDefault(recipe => recipe.CanCraft(_inputInventory));
    		return;
    	}
    	
    	_time += (float)delta;

    	if (GetProgressPercent() < 1.0f) return;
    	_time = 0;
	    _recipe.Craft(_inputInventory, _outputInventory);
	    _recipe = null;
    }
        
    public override float GetProgressPercent()
    {
	    if (_recipe == null) return 0;
	    return _time / _recipe.Time;
    }

	/**************************************************************************
	 * Protected Methods                                                      *
	 **************************************************************************/
    
    protected virtual bool MaxOutputItemsReached()
    {
	    return GetOutputInventory().CountAllItems() >= 200;
    }
    
    #region IInteractable Implementation
    /**************************************************************************
	 * IInteractable Methods                                                  *
	 **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
	    GUI.Instance.Display(playerInventory, this);
    }
    #endregion
    
    #region ICrafter Implementation
	/**************************************************************************
	 * ICrafter Methods                                                       *
	 **************************************************************************/
    public Recipe GetRecipe() => _recipe;
    public void SetRecipe(Recipe recipe) => _recipe = recipe;
    public Inventory GetInputInventory() => _inputInventory;
    public Inventory GetOutputInventory() => _outputInventory;
    public virtual string GetCategory() => "smelting";
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1)
    {
	    var itemCount = _inputInventory.CountItem(item);
	    return Globals.Database.Recipes
		    .Where(recipe => recipe.Category == GetCategory())
		    .Where(recipe => recipe.Ingredients.ContainsKey(item))
		    .Select(recipe => recipe.Ingredients)
		    .Any(ingredients => itemCount < ingredients[item] * 2);
    }
    
    public void Insert(string item, int count = 1) => _inputInventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _outputInventory.Remove(item, count);
    public List<string> GetItems() => _outputInventory.GetItems();
    public string GetFirstItem() => _outputInventory.GetFirstItem();
    public List<Inventory> GetInventories() => new() { _inputInventory, _outputInventory };

    #endregion
}
