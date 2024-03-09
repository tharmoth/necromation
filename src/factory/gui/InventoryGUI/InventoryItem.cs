using Godot;
using System;
using Necromation;

public partial class InventoryItem : ItemBox
{
	private Inventory _targetInventory;
	private Inventory _sourceInventory;
	private string _cachedSelected = "";

	public void Init(Inventory source, Inventory target, string item, int count)
	{
		ItemType = item;
		CountLabel.Text = count.ToString();
		_targetInventory = target;
		_sourceInventory = source;
		
		Button.Pressed += () =>
		{
			if (_targetInventory == null || _sourceInventory == null || !_targetInventory.CanAcceptItems(ItemType))
			{
				Globals.Player.Selected = ItemType;
				return;
			}
			var sourceCount = Input.IsActionPressed("shift") ? _sourceInventory.Items[ItemType] : 1;
			var targetCapacity = _targetInventory.GetMaxTransferAmount(ItemType);
			var amountToTransfer = Mathf.Min(sourceCount, targetCapacity);
			
			// TODO: Figure this out!
			Inventory.TransferItem(_sourceInventory, _targetInventory, ItemType, amountToTransfer);
		};
	}

	protected override void UpdateIcon()
	{
		if (Globals.Player.Selected == ItemType && !string.IsNullOrEmpty(ItemType))
		{
			Icon.Visible = true;
			Icon.Texture = Database.Instance.GetTexture("BoneHand");
			return;
		}
		base.UpdateIcon();
	}
	
	// Switch the icon to the BoneHand when the item is selected.
	public override void _Process(double delta)
	{
		base._Process(delta);

		if (_cachedSelected == Globals.Player.Selected || string.IsNullOrEmpty(ItemType)) return;
		_cachedSelected = Globals.Player.Selected;
		UpdateIcon();
	}
}
