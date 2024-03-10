using Godot;
using Necromation.sk;

namespace Necromation.factory.character.Actions;

public class InsertItemAction
{
    private readonly Inventory _inventory;
    
    public InsertItemAction(Inventory inventory)
    {
        _inventory = inventory;
    }
    
    public bool ShouldInsert(LayerTileMap.IEntity building)
    {
        return !string.IsNullOrEmpty(Globals.Player.Selected) && Input.IsMouseButtonPressed(MouseButton.Left) &&
               Input.IsKeyPressed(Key.Ctrl) &&
               building is ITransferTarget transfer &&
               transfer.GetMaxTransferAmount(Globals.Player.Selected) > 0;
    }

    public void Insert(LayerTileMap.IEntity building)
    {
        if (building is not ITransferTarget transfer) return;
        
        var count = 0;
	
        count = Mathf.Min(transfer.GetMaxTransferAmount(Globals.Player.Selected), _inventory.CountItem(Globals.Player.Selected));
        Inventory.TransferItem(_inventory, transfer.GetInputInventory(), Globals.Player.Selected, count);

        var remaining = _inventory.CountItem(Globals.Player.Selected);

        SKFloatingLabel.Show("-" + count + " " + Globals.Player.Selected + " (" + remaining + ")" , ((Building)transfer).GlobalPosition);
		
        if (remaining == 0) Globals.Player.Selected = "";
	
        MusicManager.PlayCraft();

    }
}