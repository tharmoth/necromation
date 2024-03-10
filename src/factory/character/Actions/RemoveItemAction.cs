using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interactables.belts;
using Necromation.sk;

namespace Necromation.factory.character.Actions;

public class RemoveItemAction
{
    private readonly Inventory _inventory;
    
    public RemoveItemAction(Inventory inventory)
    {
        _inventory = inventory;
    }
    
    public bool ShouldRemove(LayerTileMap.IEntity building)
    {
        return string.IsNullOrEmpty(Globals.Player.Selected) && Input.IsMouseButtonPressed(MouseButton.Left) &&
               Input.IsKeyPressed(Key.Ctrl) &&
               building is ITransferTarget transfer &&
               GetRemoveInventories(transfer).Any(inventory => inventory.CountAllItems() > 0);
    }
    
    public void Remove(LayerTileMap.IEntity building)
    {
        if (building is not ITransferTarget transfer) return;
        var inventories = GetRemoveInventories(transfer);
        var index = 0;
        foreach (var from in inventories)
        {
            foreach (var item in from.GetItems())
            {
                var count = from.CountItem(item);
                Inventory.TransferItem(from, _inventory, item, count);
                var remaining = _inventory.CountItem(item);
                SKFloatingLabel.Show("+" + count + " " + item + " (" + remaining + ")", ((Building)transfer).GlobalPosition + new Vector2(0, index++ * 20));
            }
        }

        MusicManager.PlayCraft();
    }
    
    private static List<Inventory> GetRemoveInventories(ITransferTarget transfer)
    {
        return transfer is Belt 
            ? transfer.GetInventories() 
            : new List<Inventory> { transfer.GetOutputInventory() };
    }
}