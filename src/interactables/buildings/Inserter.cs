using System;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation;

public partial class Inserter : Building, IRotatable
{
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Inserter";
    private double _time = 0;
    private double _interval = .83;
    private Tween _tween;
    private Sprite2D SpriteInHand = new()
    {
        Scale = new Vector2(0.5f, 0.5f),
        Visible = false,
        ZIndex = 1
    };
    
    private IRotatable.BuildingOrientation _orientation;
    public IRotatable.BuildingOrientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            RotationDegrees = IRotatable.GetDegreesFromOrientation(value);
        }
    }
    
    private Vector2I position => Globals.TileMap.GlobalToMap(GlobalPosition);
    private Vector2I _input => position + Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    private Vector2I _output => position + Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(-1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };

    public Inserter()
    {
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
        SpriteInHand.Position = -BuildingTileMap.TileSize / 2 * new Vector2I(0, 1);
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
        switch (sourceObject, targetObject)
        {
            case (ITransferTarget from, ITransferTarget to):
            {
                var item = from.GetFirstItem();
                if (string.IsNullOrEmpty(item) || !to.CanAcceptItems(item)) return false;
                return Inventory.TransferItem(from, to, item);
            }
        }
        return false;
    }

    private string GetSourceItem()
    {
        var sourceEntity = Globals.TileMap.GetEntities(_input, BuildingTileMap.LayerNames.Buildings);
        return sourceEntity switch
        {
            ITransferTarget building => building.GetFirstItem(),
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