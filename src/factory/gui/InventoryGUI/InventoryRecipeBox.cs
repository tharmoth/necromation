using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class InventoryRecipeBox : ItemBox
{
	private Inventory TargetInventory { get; set; } = new();
	
	private Recipe _recipe = new();

	public void Init(Inventory inventory, Recipe recipe)
	{
		TargetInventory = inventory;
		_recipe = recipe;
		ItemType = _recipe.Products.Keys.First();
		
		Button.Pressed += () => _recipe.Craft(TargetInventory);
		Button.Pressed += MusicManager.PlayCraft;
		
		TargetInventory?.Listeners.Add(Update);
		Update();
		
		IngrediantsPopup.Register(_recipe, Button);
	}

	private void Update()
	{
		if (TargetInventory == null) return;
		Button.Disabled = !_recipe.CanCraft(TargetInventory);
		Icon.Modulate = _recipe.CanCraft(TargetInventory) ? Colors.White : new Color(.5f, .5f, .5f);
		var currentCraftableCount = int.MaxValue;
		foreach (var (item, count) in _recipe.Ingredients)
		{
			currentCraftableCount = Math.Min(currentCraftableCount, TargetInventory.CountItem(item) / count);
		}
		CountLabel.Text = currentCraftableCount.ToString();
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		TargetInventory?.Listeners.Remove(Update);
	}
}
