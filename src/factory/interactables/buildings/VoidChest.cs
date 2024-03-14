using Godot;

namespace Necromation;

public class VoidChest : StoneChest
{
    public override string ItemType => "Void Chest";

    public override void _Ready()
    {
        base._Ready();
        Sprite.Modulate = Colors.Black;
        _inventory = new VoidInventory();
    }
    
    private class VoidInventory : Inventory
    {
        public override void Insert(string item, int count = 1) 
        {
            // Just don't    
        }
    }
}