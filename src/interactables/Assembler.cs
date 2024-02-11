using Necromation.gui;
using Necromation.interfaces;

namespace Necromation;

public partial class Assembler : Building, ICrafter, IInteractable, Inserter.ITransferTarget
{
    private Recipe _recipe;
    	private Inventory _inputInventory = new Inventory();
    	private Inventory _outputInventory = new Inventory();
    	private float _time;
    
    	public override void _Process(double delta)
    	{
    		base._Process(delta);
    		
    		if (_recipe == null || !_recipe.CanCraft(_inputInventory))
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
    
    	public Recipe GetRecipe(Recipe recipe)
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
    
    	public virtual string GetCategory()
    	{
    		return "None";
    	}
    
    	public override float GetProgressPercent()
    	{
    		if (_recipe == null) return 0;
    		return _time / _recipe.Time;
    	}
    
    	public override string ItemType => "Assembler";
    	public override bool CanRemove()
    	{
    		return true;
    	}
}