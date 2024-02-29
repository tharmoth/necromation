﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.gui;
using Necromation.interfaces;

namespace Necromation;

public partial class Assembler : Building, ICrafter, IInteractable, ITransferTarget
{
	public override Vector2I BuildingSize => Vector2I.One * 3;
	public override string ItemType { get; }
	public Recipe Recipe;
    private Inventory _inputInventory = new();
    private Inventory _outputInventory = new();
    private float _time;
	private readonly string _category;
	private Sprite2D _recipeSprite = new()
	{
		ZIndex = 2
	};
	private Sprite2D _outlineSprite = new()
	{
		ZIndex = 1,
		Visible = false,
		Texture = Globals.Database.GetTexture("dark760")
	};

	public Assembler(string itemType, string category)
	{
		ItemType = itemType;
	    _category = category;
	    AddChild(_recipeSprite);
	    AddChild(_outlineSprite);
	}

    public override void _Process(double delta)
    {
    	base._Process(delta);
    	
    	if (Recipe == null || !Recipe.CanCraft(_inputInventory) || MaxOutputItemsReached())
    	{
    		_time = 0;
    		return;
    	}
    	
    	_time += (float)delta;

    	if (GetProgressPercent() < 1.0f) return;
    	_time = 0;
    	Recipe.Craft(_inputInventory, _outputInventory);
    }
        
    public override float GetProgressPercent()
    {
	    if (Recipe == null) return 0;
	    return _time / Recipe.Time;
    }

	/**************************************************************************
	 * Protected Methods                                                      *
	 **************************************************************************/
    
    protected virtual bool MaxOutputItemsReached()
    {
	    return _outputInventory.CountItem(Recipe.Products.First().Key) > Recipe.Products.First().Value * 2;
    }
    
    #region IInteractable Implementation
    /**************************************************************************
	 * IInteractable Methods                                                  *
	 **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
	    if (Recipe == null)
	    {
		    FactoryGUI.Instance.Display(this);
	    }
	    else
	    {
		    FactoryGUI.Instance.Display(playerInventory, this);
	    }
    }
    #endregion
    
    #region ICrafter Implementation
	/**************************************************************************
	 * ICrafter Methods                                                       *
	 **************************************************************************/
    public Recipe GetRecipe() => Recipe;
    public void SetRecipe(Recipe recipe)
    {
	    _outlineSprite.Visible = true;
	    _outlineSprite.Scale = new Vector2(48 / _outlineSprite.Texture.GetSize().X, 48 / _outlineSprite.Texture.GetSize().Y);
	    _outlineSprite.Position = new Vector2(0, -10);
	    _recipeSprite.Texture = Globals.Database.GetTexture(recipe.Products.First().Key);
	    _recipeSprite.Scale = new Vector2(32 / _recipeSprite.Texture.GetSize().X, 32 / _recipeSprite.Texture.GetSize().Y);
	    _recipeSprite.Position = new Vector2(0, -10);
	    Recipe = recipe;
    }

    public Inventory GetInputInventory() => _inputInventory;
    public Inventory GetOutputInventory() => _outputInventory;
    public virtual string GetCategory() => _category;
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1)
    {
	    if (GetRecipe() == null) return false;
	    var ingredients = GetRecipe().Ingredients;
	    var itemCount = _inputInventory.CountItem(item);
	    return ingredients.ContainsKey(item) && itemCount < ingredients[item] * 2;
    }
    
    public void Insert(string item, int count = 1) => _inputInventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _outputInventory.Remove(item, count);
    public List<string> GetItems() => _outputInventory.GetItems();
    public string GetFirstItem() => _outputInventory.GetFirstItem();
    public List<Inventory> GetInventories() => new() { _inputInventory, _outputInventory };

    #endregion
    
    public override Godot.Collections.Dictionary<string, Variant> Save()
    {
	    return new Godot.Collections.Dictionary<string, Variant>()
	    {
		    { "ItemType", ItemType },
		    { "PosX", Position.X }, // Vector2 is not supported by JSON
		    { "PosY", Position.Y },
		    { "Orientation", Orientation.ToString() },
		    { "Recipe", Recipe.Name}
	    };
    }
}