using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.gui;
using Necromation.interactables.interfaces;
using Necromation.interfaces;

public partial class Furnace : Building, ITransferTarget, ICrafter, IInteractable
{
	/**************************************************************************
	 * Building Implementation                                                *
	 **************************************************************************/
	public override string ItemType => "Stone Furnace";
	public override Vector2I BuildingSize => Vector2I.One * 2;
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private float _time;
    private Recipe _recipe;
    private readonly Inventory _inputInventory;
    private readonly Inventory _outputInventory = new();
    private readonly Dictionary<string, int> _maxItems = new();

    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private static readonly PackedScene ParticleScene = GD.Load<PackedScene>("res://src/factory/interactables/buildings/smoke.tscn");
    private readonly GpuParticles2D _particles;
    private readonly PointLight2D _light = new();
    private Tween _animationTween;
    
    /**************************************************************************
	 * Data Constants                                                         *
	 **************************************************************************/
    private const int MaxInputItems = 50;

    public Furnace()
	{
	    _inputInventory = new FurnaceInventory(this);
	    
	    _particles = ParticleScene.Instantiate<GpuParticles2D>();
	    _particles.Emitting = false;
	    Sprite.AddChild(_particles);
	    
	    _light.Visible = false;
	    _light.Texture = Database.Instance.GetTexture("SoftLight");
	    _light.Color = Colors.Yellow;
	    _light.TextureScale = .3f;
	    _light.Position = new Vector2(0, 24);
	    Sprite.AddChild(_light);
	    
	    UpdateAllowedIngredients();
	}
    
    public override void _Ready()
    {
	    base._Ready();
	    
	    // Add this here because we need to remove it when the Sprite is removed from the tree.
	    UpdateAllowedIngredients();
	    Globals.ResearchListeners.Add(UpdateAllowedIngredients);
    }
    
    public override void _Process(double delta)
    {
	    if (!BuildComplete) return;
	    
	    base._Process(delta);
    	if (_recipe == null || !_recipe.CanCraft(_inputInventory) || MaxOutputItemsReached())
    	{
    		_time = 0;
		    _recipe = Database.Instance.UnlockedRecipes
			    .Where(recipe => recipe.Category == GetCategory())
			    .FirstOrDefault(recipe => recipe.CanCraft(_inputInventory));
		    if (_recipe == null || !_recipe.CanCraft(_inputInventory) || MaxOutputItemsReached())
		    {
			    StopWorkingAnimation();
		    }
    		return;
    	}

    	_time += (float)delta;
	    PlayWorkingAnimation();

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
    
    public override void Remove(Inventory to, bool quietly = false)
    {
	    base.Remove(to, quietly);
	    StopWorkingAnimation();
    }

	/**************************************************************************
	 * Protected Methods                                                      *
	 **************************************************************************/
    protected virtual bool MaxOutputItemsReached()
    {
	    return GetOutputInventory().CountItems() >= MaxInputItems;
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private void PlayWorkingAnimation()
    {
	    if (GodotObject.IsInstanceValid(_animationTween) && _animationTween.IsRunning()) return;

	    _animationTween?.Kill();
	    _animationTween = Globals.Tree.CreateTween();
	    _animationTween.TweenProperty(Sprite, "scale", new Vector2(1.0f, .85f), 1f);
	    _animationTween.TweenProperty(Sprite, "scale", Vector2.One, 1f);
	    _animationTween.TweenCallback(Callable.From(() => _animationTween.Kill()));
	    
	    _particles.Emitting = true;
	    _light.Visible = true;
    }

    private void StopWorkingAnimation()
    {
	    _animationTween?.Kill();
	    _particles.Emitting = false;
	    _light.Visible = false;
    }
    
    // Caches the maximum amount of items that can be inserted into the furnace by reading unlocked recipes.
    private void UpdateAllowedIngredients()
    {
	    var ingredients = Database.Instance.UnlockedRecipes
		    .Where(recipe => recipe.Category == GetCategory())
		    .SelectMany(recipe => recipe.Ingredients)
		    .ToList();
	    
	    _maxItems.Clear();
	    foreach (var (item, count) in ingredients)
	    {
		    _maxItems.TryGetValue(item, out var currentCount);
		    if (currentCount < count * 2 )
		    {
			    _maxItems[item] = count * 2;
		    }
	    }
    }
    
    #region IInteractable Implementation
    /**************************************************************************
	 * IInteractable Methods                                                  *
	 **************************************************************************/
    public void Interact(Inventory playerInventory)
    {
	    AssemblerGui.Display(playerInventory, this);
    }
    #endregion
    
    #region ICrafter Implementation
	/**************************************************************************
	 * ICrafter Methods                                                       *
	 **************************************************************************/
    public Recipe GetRecipe() => _recipe;
    // Assemblers add their contents to the players inventory when the recipe is changed. Furnaces auto choose recipes
    // so they don't need to.
    public void SetRecipe(Inventory dumpInventory, Recipe recipe)
    {
	    _recipe = recipe;
    }
    public Inventory GetInputInventory() => _inputInventory;
    public Inventory GetOutputInventory() => _outputInventory;
    public virtual string GetCategory() => "smelting";
    #endregion
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    private class FurnaceInventory : Inventory
    {
	    private readonly Furnace _furnace;

	    public FurnaceInventory(Furnace furnace)
	    {
		    _furnace = furnace;
	    }
	    
	    public override int GetMaxTransferAmount(string itemType)
	    {
		    var currentCount = CountItem(itemType);
		    if (_furnace._maxItems.ContainsKey(itemType) && currentCount < MaxInputItems)
		    {
			    return MaxInputItems - currentCount;
		    }

		    return 0;
	    }
    }
    
    public bool CanAcceptItems(string item, int count = 1) => _inputInventory.CanAcceptItems(item, count);
    public bool CanAcceptItemsInserter(string item, int count = 1) => _inputInventory.CanAcceptItemsInserter(item, count);
    public void Insert(string item, int count = 1) => _inputInventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _outputInventory.Remove(item, count);
    public List<string> GetItems() => _outputInventory.GetItems();
    public string GetFirstItem() => _outputInventory.GetFirstItem();
    public List<Inventory> GetInventories() => new() { _inputInventory, _outputInventory };
    public int GetMaxTransferAmount(string itemType) => _inputInventory.GetMaxTransferAmount(itemType);
    #endregion

    public override void _ExitTree()
    {
	    base._ExitTree();
	    Globals.ResearchListeners.Remove(UpdateAllowedIngredients);
    }
}
