using Godot;

namespace Necromation;

public partial class Barracks : StoneChest
{
    
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Barracks";
    public override bool CanAcceptItems(string item, int count = 1)
    {
        return item == "Warrior" && GetInventories().TrueForAll(inventory => inventory.CountItem(item) < 10);
    }
}