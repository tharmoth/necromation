using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.components;
using Necromation.factory.gui;
using Necromation.gui;

public partial class FurnaceGui : DeferredUpdate 
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/FurnaceGui/FurnaceGui.tscn");

	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private Container SourceInventoryItemList => GetNode<Container>("%SourceInventoryItemList");
	private Container OutputInventoryItemList => GetNode<Container>("%OutputInventoryItemList");
	private ItemSelectionItemBox ItemSelectionItemBox => GetNode<ItemSelectionItemBox>("%ItemSelectionItemBox");
	private ProgressTracker ProgressBar => GetNode<ProgressTracker>("%ProgressBar");
	private Label Title => GetNode<Label>("%Title");
	private TextureRect BuildingTexture => GetNode<TextureRect>("%BuildingTexture");
	private ProgressTracker FuelProgressBar => GetNode<ProgressTracker>("%FuelProgressBar");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _to;
	private Furnace _crafter;
	
	// Static Accessor
	public static void Display(Inventory to, Furnace crafter)
	{
		var gui = Scene.Instantiate<FurnaceGui>();
		gui.Init(to, crafter);
		Globals.FactoryScene.Gui.Open(gui);
	}

	// Constructor workaround.
	private void Init(Inventory to, Furnace crafter)
	{
		_to = to;
		_crafter = crafter;

		AddUpdateListeners(new List<Inventory> { _to, _crafter.GetInputInventory(), _crafter.GetOutputInventory() });
		crafter.RecipeListeners.Add(FlagDirty);
		
		ProgressBar.Init(_crafter.GetProgressPercent);
		FuelProgressBar.Init(() => _crafter.GetComponent<FuelComponent>().Progress);
		
		Title.Text = _crafter.ItemType;
		
		BuildingTexture.Texture = Database.Instance.GetTexture(_crafter.ItemType);
	}

	protected override void Update()
	{
		InventoryItemBox.UpdateInventory(_to, new List<Inventory> { _crafter.GetInputInventory()  }, InventoryItemList);
		UpdateInventory(_crafter.GetInputInventory(), new List<Inventory> { _to }, SourceInventoryItemList, _crafter, true);
		UpdateInventory(_crafter.GetOutputInventory(), new List<Inventory> { _to }, OutputInventoryItemList, _crafter, false);
		Dirty = false;
	}
	
	private static void UpdateInventory(Inventory from, List<Inventory> to, Container list, Furnace furnace, bool isInput)
	{
		// Acts like an assembler item box when the furnace has a recipe.
		// Otherwise displays placeholders.
		if (furnace.GetRecipe() != null)
		{
			list.GetChildren().Where(node => node is not AssemblerItemBox).ToList().ForEach(node => node.QueueFree());
			if (isInput)
			{
				AssemblerItemBox.UpdateInventory(furnace.GetInputInventory(), to, list, furnace.GetRecipe(), true);
				AssemblerItemBox.AddInventoryItem("Coal Ore", furnace.GetInputInventory().CountItem("Coal Ore"), from, to, list, true);
			}
			else AssemblerItemBox.UpdateInventory(furnace.GetOutputInventory(), to, list, furnace.GetRecipe(), false);
		}
		else if (isInput)
		{
			list.GetChildren().ToList().ForEach(node => node.QueueFree());

			var validItems = furnace.MaxItems.Keys.ToList();
			validItems.Remove("Coal Ore");
			FilterItemBox.AddItemBox(from, to, validItems, list);
			
			AssemblerItemBox.AddInventoryItem("Coal Ore", 2, from, to, list, true);
		}
		else
		{
			list.GetChildren().ToList().ForEach(node => node.QueueFree());
			FilterItemBox.AddItemBox(from, to, new List<string>(), list);
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_crafter.RecipeListeners.Remove(FlagDirty);
	}
}
