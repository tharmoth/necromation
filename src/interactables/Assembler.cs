using System.Linq;
using Necromation.gui;
using Necromation.interfaces;

namespace Necromation;

public partial class Assembler : Building, ICrafter, IInteractable, Inserter.ITransferTarget
{
	public override string ItemType => "Assembler";
    private Recipe _recipe;
    private Inventory _inputInventory = new Inventory();
    private Inventory _outputInventory = new Inventory();
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

    protected virtual bool MaxOutputItemsReached()
    {
	    return _outputInventory.CountItem(_recipe.Products.First().Key) > _recipe.Products.First().Value * 2;
    }
    

    public Recipe GetRecipe()
    {
    	return _recipe;
    }

    public void SetRecipe(Recipe recipe)
    {
    	_recipe = recipe;
    }

    public Inventory GetInputInventory()
    {
    	return _inputInventory;
    }

    public Inventory GetOutputInventory()
    {
    	return _outputInventory;
    }

    public bool CanAcceptItem(string item)
    {
	    if (GetRecipe() == null) return false;
	    var ingredients = GetRecipe().Ingredients;
	    var count = GetInputInventory().CountItem(item);
	    return ingredients.ContainsKey(item) && count < ingredients[item] * 2;
    }

    public virtual string GetCategory()
    {
    	return "None";
    }

    public override float GetProgressPercent()
    {
    	if (_recipe == null) return 0;
    	return _time / _recipe.Time;
    }
}