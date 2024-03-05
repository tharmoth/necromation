using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.gui;
using Necromation.interactables.interfaces;
using Necromation.interfaces;

public partial class Furnace : Building, ITransferTarget, ICrafter, IInteractable
{
	public override Vector2I BuildingSize => Vector2I.One * 2;
	public override string ItemType => "Stone Furnace";
    private Recipe _recipe;
    private Inventory _inputInventory = new();
    private Inventory _outputInventory = new();
    private float _time;

    private GpuParticles2D _particles = GD.Load<PackedScene>("res://src/interactables/buildings/smoke.tscn")
	    .Instantiate<GpuParticles2D>();

    public override void _Ready()
    {
	    base._Ready();
	    CallDeferred("add_child", _particles);
    }

    public override void _Process(double delta)
    {
    	base._Process(delta);
	    
    	if (_recipe == null || !_recipe.CanCraft(_inputInventory) || MaxOutputItemsReached())
    	{
    		_time = 0;
		    _recipe = Database.Instance.Recipes
			    .Where(recipe => recipe.Category == GetCategory())
			    .FirstOrDefault(recipe => recipe.CanCraft(_inputInventory));
		    tweenytwiney?.Kill();
		    _particles.Emitting = false;
    		return;
    	}
    	
	    _particles.Emitting = true;
    	_time += (float)delta;
	    Animate();

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
    
    private Tween tweenytwiney;
    
    private void Animate()
    {
	    if (IsInstanceValid(tweenytwiney) && tweenytwiney.IsRunning()) return;
        
	    // Get random position
	    var randomPosition = new Vector2((float)GD.RandRange(-2.0f, 2.0f), (float)GD.RandRange(-3.0f, 0f));
	    tweenytwiney?.Kill();
	    tweenytwiney = GetTree().CreateTween();
	    tweenytwiney.TweenProperty(Sprite, "scale", new Vector2(1.0f, .85f), 1f);
	    tweenytwiney.TweenProperty(Sprite, "scale", Vector2.One, 1f);
	    // tweenytwiney.TweenProperty(Sprite, "scale", Vector2.One, .5f);
	    tweenytwiney.TweenCallback(Callable.From(() => tweenytwiney.Kill()));
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
	    Globals.FactoryScene.Gui.Display(playerInventory, this);
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
	    return Database.Instance.Recipes
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
    public int GetMaxTransferAmount(string itemType)
    {
	    return Database.Instance.Recipes
		    .Where(recipe => recipe.Category == GetCategory())
		    .Where(recipe => recipe.Ingredients.ContainsKey(itemType))
		    .Select(recipe => recipe.Ingredients)
		    .Where(ingredient => ingredient.ContainsKey(itemType))
		    .Select(_ => GetInputInventory().CountItem(itemType))
		    .Select(count => 50 - count)
		    .Max();
    }

    #endregion
}
