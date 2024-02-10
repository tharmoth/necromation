using System;
using System.Linq;
using Godot;

namespace Necromation;

public partial class Inserter : Building
{
    
    private double _time = 0;
    private double _interval = 1.0;
    private Tween _tween;

    Orientation _orientation = Orientation.SouthNorth;
    public enum Orientation
    {
        NorthSouth,
        EastWest,
        SouthNorth,
        WestEast
    }
    
    public Inserter(int degrees)
    {
        _orientation = degrees switch {
            0 => Orientation.NorthSouth,
            90 => Orientation.EastWest,
            180 => Orientation.SouthNorth,
            270 => Orientation.WestEast,
            _ => throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null)
        };
        
        RotationDegrees = degrees;
    }
    
    public Inserter(Orientation orientation)
    {
        _orientation = orientation;
    }
    
    public override string ItemType => "Inserter";
    public override bool CanRemove()
    {
        return true;
    }

    public override float GetProgressPercent()
    {
        return 0;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _time += delta;
        if (_time < _interval) return;
        _time = 0;

        var position = Globals.TileMap.GlobalToMap(GlobalPosition);
        
        var input = position + _orientation switch {
            Orientation.NorthSouth => new Vector2I(0, -1),
            Orientation.EastWest => new Vector2I(1, 0),
            Orientation.SouthNorth => new Vector2I(0, 1),
            Orientation.WestEast => new Vector2I(-1, 0),
            _ => throw new ArgumentOutOfRangeException()
        };
        var output = position + _orientation switch {
            Orientation.NorthSouth => new Vector2I(0, 1),
            Orientation.EastWest => new Vector2I(-1, 0),
            Orientation.SouthNorth => new Vector2I(0, -1),
            Orientation.WestEast => new Vector2I(1, 0),
            _ => throw new ArgumentOutOfRangeException()
        };

        var inputBuilding = Globals.TileMap.GetEntities(input, BuildingTileMap.LayerNames.Buildings) as ITransferTarget;
        var outputBuilding = Globals.TileMap.GetEntities(output, BuildingTileMap.LayerNames.Buildings) as ITransferTarget;;
        
        if (inputBuilding == null || outputBuilding == null) return;
        
        var item = inputBuilding.GetOutputInventory().Items.Keys.FirstOrDefault();

        if (string.IsNullOrEmpty(item)) return;

        Inventory.TransferItem(inputBuilding.GetOutputInventory(), outputBuilding.GetInputInventory(), item);

        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(this, "rotation", Rotation + Math.PI, 0.5f);
        _tween.TweenProperty(this, "rotation", Rotation, 0.5f);
    }

    public interface ITransferTarget
    {
        /*
         * Returns the inventory that the crafter uses to get the ingredients
         */
        public Inventory GetInputInventory();
    
        /*
         * Returns the inventory that the crafter uses to store the products
         */
        public Inventory GetOutputInventory();
    }
}