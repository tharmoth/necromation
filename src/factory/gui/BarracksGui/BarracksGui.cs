using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.factory.gui;
using Necromation.gui;

public partial class BarracksGui : DeferredUpdate
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/BarracksGui/BarracksGui.tscn");

	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private Container UnitsList => GetNode<Container>("%UnitsList");
	private Label Title => GetNode<Label>("%Title");
	private Label UnitCountLabel => GetNode<Label>("%UnitCountLabel");
	private CommandsGui CommandsGui => GetNode<CommandsGui>("%CommandsGui");
	private BarracksBoxDraw BarracksBoxDraw => GetNode<BarracksBoxDraw>("%BarracksBoxDraw");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Inventory _to;
	private Barracks _barracks;
	
	// Static Accessor
	public static void Display(Inventory to, Barracks barracks)
	{
		var gui = Scene.Instantiate<BarracksGui>();
		gui.Init(to, barracks);
		Globals.FactoryScene.Gui.Open(gui);
	}

	// Constructor workaround.
	private void Init(Inventory to, Barracks barracks)
	{
		_to = to;
		_barracks = barracks;

		AddUpdateListeners(new List<Inventory> { _to, _barracks.Inventory});

		Title.Text = barracks.ItemType + " - " + barracks.Commander.CommanderName;

		CommandsGui.Init(barracks.Commander);
		BarracksBoxDraw.Init(barracks.Commander);
	}

	protected override void Update()
	{
		InventoryItemBox.UpdateInventory(_to, new List<Inventory> { _barracks.Inventory }, InventoryItemList);
		// InventoryItem.UpdateInventory(_barracks.GetInputInventory(), new List<Inventory> { _to }, SourceInventoryItemList);
		// InventoryItemBox.UpdateInventory(_barracks.Inventory, new List<Inventory> { _to }, OutputInventoryItemList);
		UnitCountLabel.Text = _barracks.Inventory.CountItems().ToString() + " / " + _barracks.Commander.CommandCap;
		Dirty = false;

		UnitsList.GetChildren().ToList().ForEach(node => node.QueueFree());

		int count = 0;
		foreach (var unit in _barracks.Inventory.Items.Keys)
		{
			AssemblerItemBox.AddInventoryItem(unit, 
				0, 
				_barracks.Inventory, 
				new List<Inventory> { _to }, 
				UnitsList, 
				true);
			count++;
		}

		var valid = Database.Instance.Units.Select(unit => unit.Name).Where(unit =>
		{
			return Database.Instance.UnlockedRecipes
				.Any(recipe => recipe.Products.Any(product => product.Key == unit));
		}).ToList();
		for (var i = 0; i < 8 - count; i++)
		{
			FilterItemBox.AddItemBox(_barracks.Inventory, 
				new List<Inventory> { _to }, 
				valid, 
				UnitsList);
		}
	}
}
