using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public class Loader : Belt
{
    public override string ItemType => "Loader";
    public Loader(IRotatable.BuildingOrientation orientation) : base(orientation)
    {
        Sprite.Modulate = Colors.Green;
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);


        // UnLoad
        var from = Globals.FactoryScene.TileMap.GetEntity(Input, FactoryTileMap.Building) as ITransferTarget;
        if (from is not null && from is not Belt)
        {
            var leftItem = from.GetItems().FirstOrDefault();
            if (!string.IsNullOrEmpty(leftItem) && CanInsertLeft(leftItem))
            {
                from.Remove(leftItem);
                InsertLeft(leftItem);
            }

            var rightItem = from.GetItems().FirstOrDefault();
            if (!string.IsNullOrEmpty(rightItem) && CanInsertRight(rightItem))
            {
                from.Remove(rightItem);
                InsertRight(rightItem);
            }
        }
        
        // Load
        var to = Globals.FactoryScene.TileMap.GetEntity(Output, FactoryTileMap.Building) as ITransferTarget;
        if (to is not null or Belt)
        {
            foreach (var item in GetItems())
            {
                if (string.IsNullOrEmpty(item) || !to.CanAcceptItems(item)) continue;
                Inventory.TransferItem(this, to, item);
            }
        }
    }
}