using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class Mine : Building, IInteractable, Inserter.ITransferTarget
{
    private Inventory _inventory = new();
    private float _time;
    private float miningSpeed = 1.0f;

    public override string ItemType => "Mine";

    public override void _Process(double delta)
    {
        base._Process(delta);

        _time += (float)delta;

        if (GetProgressPercent() < 1.0f) return;
        _time = 0;
        
        var resource = Globals.TileMap.GetEntities(Globals.TileMap.GlobalToMap(GlobalPosition), BuildingTileMap.LayerNames.Resources);
        if (resource is not Collectable collectable) return;
        
        _inventory.AddItem(collectable.ItemType);
    }
    
    public override float GetProgressPercent()
    {
        return _time / miningSpeed;
    }
    
    public override bool CanRemove()
    {
        return true;
    }

    public void Interact()
    {
         GUI.Instance.ContainerGui.Display(Globals.PlayerInventory, _inventory, ItemType);
    }

    public Inventory GetInputInventory()
    {
        return _inventory;
    }

    public Inventory GetOutputInventory()
    {
        return _inventory;
    }
}