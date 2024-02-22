using Godot;

namespace Necromation.gui;

public partial class ItemLabel : Container
{
    public string ItemType { get; set; } = "None";
    public Inventory Inventory { get; set; }

    public void Update()
    {
        GetNode<Label>("Label").Text = ItemType;
        GetNode<Label>("Counter").Text = Globals.PlayerInventory.CountItem(ItemType).ToString();
    }
}