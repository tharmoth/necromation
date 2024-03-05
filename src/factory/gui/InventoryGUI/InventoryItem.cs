using Godot;
using System;
using Necromation;

public partial class InventoryItem : PanelContainer
{
	private TextureRect Icon => GetNode<TextureRect>("%Icon");
	private Label CountLabel => GetNode<Label>("%CountLabel");
	private Button Button => GetNode<Button>("%Button");
	
	private string _itemType;
	public string ItemType
	{
		get => _itemType;
		set
		{
			_itemType = value;
			Icon.Texture = Database.Instance.GetTexture(_itemType);
		}
	}

	public int Count
	{
		get => int.Parse(CountLabel.Text);
		set => CountLabel.Text = value.ToString();
	}
	
	public Inventory TargetInventory { get; set; }
	public Inventory SourceInventory { get; set; }
	
	public override void _Ready()
	{
		base._Ready();
		Button.Pressed += () =>
		{
			if (TargetInventory == null || SourceInventory == null || !SourceInventory.CanAcceptItems(ItemType))
			{
				Globals.Player.Selected = ItemType;
				return;
			}
			Inventory.TransferItem(SourceInventory, TargetInventory, ItemType);
		};
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Could be bad for performance, But seems to work.
		Icon.Texture = Database.Instance.GetTexture(Globals.Player.Selected == ItemType ? "BoneHand" : ItemType);
	}
}
