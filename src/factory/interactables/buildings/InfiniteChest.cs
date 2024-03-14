using Godot;

namespace Necromation;

public class InfiniteChest : StoneChest
{
    public override string ItemType => "Infinite Chest";

    public override void _Ready()
    {
        base._Ready();
        Sprite.Modulate = Colors.Blue;
        _inventory = new InfiniteInventory();
    }
    
    private class InfiniteInventory : Inventory
    {
        public override bool Remove(string item, int count = 1) 
        {
            // Just don't  
            return true;
        }

        public override int GetMaxTransferAmount(string itemType)
        {
            return 1;
        }
    }
}