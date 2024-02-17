using System.Linq;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class Mine : Building, IInteractable, Inserter.ITransferTarget
{
    public override Vector2I BuildingSize => Vector2I.One * 2;
    private Inventory _inventory = new();
    private float _time;
    private float miningSpeed = 1.0f;

    public override string ItemType => "Mine";

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (MaxOutputItemsReached())
        {
            _time = 0;
            return;
        }

        _time += (float)delta;

        if (GetProgressPercent() < 1.0f) return;
        _time = 0;

        var resource = Globals.TileMap.GetEntityPositions(this)
            .Select(position => Globals.TileMap.GetEntities(position, BuildingTileMap.LayerNames.Resources))
            .First(resource => resource is Collectable);
        
        if (resource is not Collectable collectable) return;
        _inventory.Insert(collectable.ItemType);
    }
    
    private bool MaxOutputItemsReached()
    {
        return _inventory.CountAllItems() >= 200;
    }
    
    public override float GetProgressPercent()
    {
        return _time / miningSpeed;
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

    public bool CanAcceptItem(string item)
    {
        return false;
    }
}