using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.gui;

public partial class CrafterGUI : Control
{
	public static void Display(Inventory to, ICrafter crafter)
	{
		var gui = GD.Load<PackedScene>("res://src/factory/gui/CrafterGUI/CrafterGUI.tscn").Instantiate<CrafterGUI>();
		gui.Init(to, crafter);
		Globals.FactoryScene.Gui.OpenGui(gui);
	}
	
	private Inventory _to;
	private ICrafter _crafter;
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private Container SourceInventoryItemList => GetNode<Container>("%SourceInventoryItemList");
	private Container OutputInventoryItemList => GetNode<Container>("%OutputInventoryItemList");
	private Button RecipeChangeButton => GetNode<Button>("%ChangeRecipeButton");
	private ProgressTracker ProgressBar => GetNode<ProgressTracker>("%ProgressBar");
	private Label Title => GetNode<Label>("%Title");
	
	private void Init(Inventory to, ICrafter crafter)
	{
		_to = to;
		_crafter = crafter;
		
		_to.Listeners.Add(UpdatePlayerInventory);
		UpdatePlayerInventory();
		
		_crafter.GetInputInventory().Listeners.Add(UpdateSourceInventory);
		UpdateSourceInventory();
		
		_crafter.GetOutputInventory().Listeners.Add(UpdateOutputInventory);
		UpdateOutputInventory();

		RecipeChangeButton.Pressed += () =>
		{
			RecipePopup.Display(to, _crafter);
		};
		
		if (_crafter is ProgressTracker.IProgress progress)
		{
			ProgressBar.Init(progress);
		}
		
		Title.Text = _crafter.ItemType;
	}

	private void UpdatePlayerInventory()
	{
		UpdateInventory(_to, _crafter.GetInputInventory(), InventoryItemList);
	}
	
	private void UpdateSourceInventory()
	{
		UpdateInventory(_crafter.GetInputInventory(), _to, SourceInventoryItemList);
	}
	
	private void UpdateOutputInventory()
	{
		UpdateInventory(_crafter.GetOutputInventory(), _to, OutputInventoryItemList);
	}
	
	private void UpdateInventory(Inventory from, Inventory to, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());
		from.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(item => AddInventoryItem(item, from, to, list));
	}
	
	private void AddInventoryItem(KeyValuePair<string, int> item, Inventory from, Inventory to, Container list)
	{
		var inventoryItem = GD.Load<PackedScene>("res://src/factory/gui/InventoryGUI/inventory_item_box.tscn").Instantiate<InventoryItem>();
		inventoryItem.Init(from, to, item.Key, item.Value);
		list.AddChild(inventoryItem);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_to?.Listeners.Remove(UpdatePlayerInventory);
		_crafter?.GetInputInventory().Listeners.Remove(UpdateSourceInventory);
		_crafter?.GetOutputInventory().Listeners.Remove(UpdateOutputInventory);
	}
}
