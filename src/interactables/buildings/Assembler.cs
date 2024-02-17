using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.gui;
using Necromation.interfaces;

namespace Necromation;

public partial class Assembler : Building, ICrafter, IInteractable, ITransferTarget
{
	public override Vector2I BuildingSize => Vector2I.One * 3;
	public override string ItemType => "Assembler";
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
    		return;
    	}
    	
    	_time += (float)delta;

    	if (GetProgressPercent() < 1.0f) return;
    	_time = 0;
    	_recipe.Craft(_inputInventory, _outputInventory);
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
	    return _outputInventory.CountItem(_recipe.Products.First().Key) > _recipe.Products.First().Value * 2;
    }
    
    #region IInteractable Implementation
    /**************************************************************************
	 * IInteractable Methods                                                  *
	 **************************************************************************/
    public void Interact()
    {
	    if (_recipe == null)
	    {
		    GUI.Instance.Popup.DisplayPopup(this);
	    }
	    else
	    {
		    GUI.Instance.CrafterGui.Display(this);
	    }
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
    public virtual string GetCategory() => "None";
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1, Vector2 position = default)
    {
	    if (GetRecipe() == null) return false;
	    var ingredients = GetRecipe().Ingredients;
	    var itemCount = _inputInventory.CountItem(item);
	    return ingredients.ContainsKey(item) && itemCount < ingredients[item] * 2;
    }
    
    public void Insert(string item, int count = 1, Vector2 position = default) => _inputInventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _outputInventory.Remove(item, count);
    public string GetFirstItem() => _outputInventory.GetFirstItem();
    public List<Inventory> GetInventories() => new() { _inputInventory, _outputInventory };

    #endregion
}