using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.components.animation;
using Necromation.factory.gui;
using Necromation.gui;

public partial class ManaforgeGui : DeferredUpdate 
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = Database.GetScene(nameof(ManaforgeGui));

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
	private Manaforge _crafter;

	/**************************************************************************
	 * Constructor                                                            *
	 **************************************************************************/
	private void Init(Inventory to, Manaforge crafter)
	{
		_to = to;
		_crafter = crafter;

		AddUpdateListeners(new List<Inventory> { _to, _crafter.GetInventories().First() });
		
		ProgressBar.Init(_crafter.GetProgressPercent);
		FuelProgressBar.Init(GetEnergyPercent);
		
		Title.Text = _crafter.ItemType;
		
		BuildingTexture.Texture = Database.Instance.GetTexture(_crafter.ItemType);
	}
	
	/*************************************************************************
	 * Public Methods                                                        *
	 *************************************************************************/
	// Static Accessor
	public static void Display(Inventory to, Manaforge crafter)
	{
		var gui = Scene.Instantiate<ManaforgeGui>();
		gui.Init(to, crafter);
		Globals.FactoryScene.Gui.Open(gui);
	}
	
	/**************************************************************************
	 * Protected Methods                                                      *
	 **************************************************************************/
	protected override void Update()
	{
		InventoryItemBox.UpdateInventory(_to, new List<Inventory> { _crafter.GetInventories().First()  }, InventoryItemList);
		UpdateInventory(_crafter.GetInventories().First(), new List<Inventory> { _to }, SourceInventoryItemList, _crafter, true);
		Dirty = false;
	}
	
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	private float GetEnergyPercent() => 
		_crafter.GetComponent<PowerSourceComponent>().Energy / _crafter.GetComponent<PowerSourceComponent>().EnergyMax;
	
	private static void UpdateInventory(Inventory from, List<Inventory> to, Container list, Manaforge furnace, bool isInput)
	{
		// Acts like an assembler item box when the furnace has a recipe.
		// Otherwise displays placeholders.
		list.Clear();
		AssemblerItemBox.AddInventoryItem("Coal Ore", furnace.GetInventories().First().CountItem("Coal Ore"), from, to, list, true);
	}
}
