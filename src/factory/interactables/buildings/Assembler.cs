using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.gui;
using Necromation.interactables.interfaces;
using Necromation.interfaces;

namespace Necromation;

public partial class Assembler : Building, ICrafter, IInteractable, ITransferTarget, IRotatable
{
	/**************************************************************************
	 * Building Implementation                                                *
	 **************************************************************************/
	public override string ItemType { get; }
	public override Vector2I BuildingSize => Vector2I.One * 3;
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private float _time;
	private Recipe _recipe;
	private readonly string _category;
    private Inventory _inputInventory;
    private Inventory _outputInventory = new();
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
	private Tween _animationTween;
	private Sprite2D _recipeSprite = new()
	{
		ZIndex = 2
	};
	private Sprite2D _outlineSprite = new()
	{
		ZIndex = 1,
		Visible = false,
		Texture = Database.Instance.GetTexture("Dark760")
	};
	
	/**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
	private const int MaxInputItems = 50;
	
	public Assembler(string itemType, string category)
	{
		ItemType = itemType;
	    _category = category;
	    Sprite.AddChild(_recipeSprite);
	    Sprite.AddChild(_outlineSprite);
	    _inputInventory = new AssemblerInventory(this);
	}

	public override void _Process(double delta)
    {
	    base._Process(delta);
    	if (_recipe == null || !_recipe.CanCraft(_inputInventory) || MaxOutputItemsReached())
    	{
    		_time = 0;
    		return;
    	}
    	
    	_time += (float)delta;
	    Animate();

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
    
    private void Animate()
    {
	    if (GodotObject.IsInstanceValid(_animationTween) && _animationTween.IsRunning()) return;

	    _animationTween?.Kill();
	    _animationTween = Globals.Tree.CreateTween();
	    _animationTween.TweenProperty(Sprite, "scale", new Vector2(.85f, .85f), 1f);
	    _animationTween.TweenProperty(Sprite, "scale", Vector2.One, 1f);
	    _animationTween.TweenCallback(Callable.From(() => _animationTween.Kill()));
    }
    
    #region IInteractable Implementation
    /**************************************************************************
	 * IInteractable Methods                                                  *
	 **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
	    if (_recipe == null)
	    {
		    RecipePopup.Display(playerInventory, this);
	    }
	    else
	    {
		    CrafterGUI.Display(playerInventory, this);
	    }
    }
    #endregion
    
    #region ICrafter Implementation
	/**************************************************************************
	 * ICrafter Methods                                                       *
	 **************************************************************************/
    public Recipe GetRecipe() => _recipe;
    // Assemblers add their contents to the players inventory when the recipe is changed.
    public void SetRecipe(Inventory dumpInventory, Recipe recipe)
    {
	    if (dumpInventory != null) TransferInventories(dumpInventory);

	    _outlineSprite.Visible = true;
	    _outlineSprite.Scale = new Vector2(48 / _outlineSprite.Texture.GetSize().X, 48 / _outlineSprite.Texture.GetSize().Y);
	    _outlineSprite.Position = new Vector2(0, -10);
	    _recipeSprite.Texture = Database.Instance.GetTexture(recipe.Products.First().Key);
	    _recipeSprite.Scale = new Vector2(32 / _recipeSprite.Texture.GetSize().X, 32 / _recipeSprite.Texture.GetSize().Y);
	    _recipeSprite.Position = new Vector2(0, -10);
	    _recipe = recipe;
    }

    public Inventory GetInputInventory() => _inputInventory;
    public Inventory GetOutputInventory() => _outputInventory;
    public virtual string GetCategory() => _category;
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    private class AssemblerInventory : Inventory
    {
	    private readonly Assembler _assembler;

	    public AssemblerInventory(Assembler assembler)
	    {
		    _assembler = assembler;
	    }
	    
	    public override int GetMaxTransferAmount(string itemType)
	    {
		    if (_assembler.GetRecipe() == null) return 0;
		    
		    var ingredients = _assembler.GetRecipe().Ingredients;
		    if (!ingredients.ContainsKey(itemType)) return 0;
		    
		    var currentCount = CountItem(itemType);
		    return Mathf.Max(0,  MaxInputItems - currentCount);
	    }
    }
    
    public bool CanAcceptItems(string item, int count = 1) => _inputInventory.CanAcceptItems(item, count);
    public void Insert(string item, int count = 1) => _inputInventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _outputInventory.Remove(item, count);
    public List<string> GetItems() => _outputInventory.GetItems();
    public string GetFirstItem() => _outputInventory.GetFirstItem();
    public List<Inventory> GetInventories() => new() { _inputInventory, _outputInventory };

    #endregion
}