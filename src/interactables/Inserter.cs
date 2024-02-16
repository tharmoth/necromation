using System;
using System.Linq;
using Godot;
using Necromation.interactables.belts;

namespace Necromation;

public partial class Inserter : Building
{
    public override string ItemType => "Inserter";
    private double _time = 0;
    private double _interval = .83;
    private Tween _tween;
    private Vector2I _input;
    private Vector2I _output;
    private Sprite2D SpriteInHand = new()
    {
        Scale = new Vector2(0.5f, 0.5f),
        Visible = false,
        ZIndex = 1
    };

    public Inserter(int degrees)
    {
        _orientation = degrees switch {
            0 => BuildingOrientation.NorthSouth,
            90 => BuildingOrientation.EastWest,
            180 => BuildingOrientation.SouthNorth,
            270 => BuildingOrientation.WestEast,
            _ => throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null)
        };
        
        RotationDegrees = degrees;
        SetNotifyTransform(true);
        
        AddChild(SpriteInHand);
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what != NotificationTransformChanged) return;
        UpdateInputOutput();
    }
    
    private void UpdateInputOutput()
    {
        var position = Globals.TileMap.GlobalToMap(GlobalPosition);
        _input = position + _orientation switch {
            BuildingOrientation.NorthSouth => new Vector2I(0, -1),
            BuildingOrientation.EastWest => new Vector2I(1, 0),
            BuildingOrientation.SouthNorth => new Vector2I(0, 1),
            BuildingOrientation.WestEast => new Vector2I(-1, 0),
            _ => throw new ArgumentOutOfRangeException()
        };
        _output = position + _orientation switch {
            BuildingOrientation.NorthSouth => new Vector2I(0, 1),
            BuildingOrientation.EastWest => new Vector2I(-1, 0),
            BuildingOrientation.SouthNorth => new Vector2I(0, -1),
            BuildingOrientation.WestEast => new Vector2I(1, 0),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        SpriteInHand.Position = -BuildingTileMap.TileSize / 2 * new Vector2I(0, 1);
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

        var item = GetSourceItem();
        if (!TryTransfer()) return;
        _time = 0;
        Animate(item);
    }

    private void Animate(string item)
    {
        SpriteInHand.Texture = GD.Load<Texture2D>($"res://res/sprites/{item}.png");
        SpriteInHand.Visible = true;
        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(this, "rotation", Rotation + Math.PI, _interval/2);
        _tween.TweenCallback(new Callable(this, "DropItem"));
        _tween.TweenProperty(this, "rotation", Rotation, _interval/2);
    }

    private void DropItem()
    {
        SpriteInHand.Visible = false;
    }
    
    private bool Transfer(object sourceObject, object targetObject)
    {
        // There are five different cases to consider:
        // 1. building -> building
        // 2. building -> ground
        // 3. belt -> building
        // 4. belt -> ground
        // 5. ground -> building
        // 6. ground -> ground
        // 7. no valid targets
        
        // TODO: I don't like how this logic is handled.
        // I feel like there should be a more elegant solution. I need to think further about where this logic
        // should be handled. Maybe in the Inventory class?
        
        switch (sourceObject, targetObject)
        {
            case (ITransferTarget from, ITransferTarget to):
            {
                var item = from.GetOutputInventory().GetFirstItem();
                if (string.IsNullOrEmpty(item) || !to.CanAcceptItem(item)) return false;
                return Inventory.TransferItem(from.GetOutputInventory(), to.GetInputInventory(), item);
            }
            // case (ITransferTarget sourceBuilding, null):
            // {
            //     var item = sourceBuilding.GetOutputInventory().GetFirstItem();
            //     if (string.IsNullOrEmpty(item)) return false;
            //     return Inventory.PlaceItemOnGround(sourceBuilding.GetOutputInventory(), _output, item);
            // }
            // case (GroundItem groundItem, ITransferTarget targetBuilding):
            // {
            //     if (!targetBuilding.CanAcceptItem(groundItem.ItemType)) return false;
            //     return groundItem.AddToInventory(targetBuilding.GetInputInventory());
            // }
            // case (GroundItem groundItem, null):
            // {
            //     return Globals.TileMap.MoveEntity(groundItem, _output, BuildingTileMap.LayerNames.GroundItems);
            // }
        }
        return false;
    }

    private string GetSourceItem()
    {
        var sourceEntity = Globals.TileMap.GetEntities(_input, BuildingTileMap.LayerNames.Buildings);
        return sourceEntity switch
        {
            ITransferTarget building => building.GetOutputInventory().GetFirstItem(),
            _ => null
        };
    }

    private bool TryTransfer()
    {
        var sourceEntity = Globals.TileMap.GetEntities(_input, BuildingTileMap.LayerNames.Buildings);
        var targetEntity = Globals.TileMap.GetEntities(_output, BuildingTileMap.LayerNames.Buildings);

        return Transfer(sourceEntity, targetEntity);
    }
}