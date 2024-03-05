using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interfaces;

namespace Necromation;

public partial class TradeDepot : Building, ITransferTarget, IInteractable
{
    public override Vector2I BuildingSize { get; }
    public override string ItemType { get; }
    
    private Inventory _inventory = new();
    
    public TradeDepot(string itemType)
    {
        BuildingSize = new Vector2I(3, 3);
        ItemType = itemType;
    }

    public override void _Ready()
    {
        base._Ready();
        var depots = Globals.FactoryScene.TileMap.GetEntitiesOfType("TradeDepot").Where(entity => entity != this)
            .OfType<TradeDepot>().Where(depot => depot.ItemType != ItemType)
            .ToList();
                
        depots.ForEach(depot =>
        {
            GD.Print((MapPosition - depot.MapPosition).Length());
        });
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        // Check if the position is on the edge of a province.
        return base.CanPlaceAt(position) && GetOccupiedPositions(position)
            .Any(pos => pos.X % 100 == 0 || pos.Y % 100 == 0 || pos.X % 100 == 99 || pos.Y % 100 == 99);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (ItemType == "Exporter") return;
        
        var depots = Globals.FactoryScene.TileMap.GetEntitiesOfType("TradeDepot").Where(entity => entity != this)
            .OfType<TradeDepot>().Where(depot => depot.ItemType != ItemType)
            .Where(depot => (MapPosition - depot.MapPosition).Length() > 142)
            .ToList();

        depots.ForEach(depot =>
        {
            Inventory.TransferAllTo(depot._inventory, _inventory);
        });
    }

    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public virtual bool CanAcceptItems(string item,  int count = 1) => true;
    public void Insert(string item, int count = 1) => _inventory.Insert(item, count);
    public bool Remove(string item, int count = 1) => _inventory.Remove(item, count);
    public string GetFirstItem() => _inventory.GetFirstItem();
    public List<string> GetItems() => _inventory.GetItems();
    public List<Inventory> GetInventories() => new() { _inventory };
    #endregion

    public void Interact(Inventory playerInventory)
    {
        Globals.FactoryScene.Gui.Display(playerInventory, _inventory, ItemType);
    }
}