﻿using System;
using Godot;
using Necromation.interactables.belts;
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
        if (!Transfer()) return;
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

    private bool Transfer()
    {
        var sourceEntity = Globals.TileMap.GetEntities(_input, BuildingTileMap.Building);
        var targetEntity = Globals.TileMap.GetEntities(_output, BuildingTileMap.Building);
        
        switch (sourceEntity, targetEntity)
        {
            case (ITransferTarget from, DoubleBelt to):
            {
                var item = from.GetFirstItem();
                if (string.IsNullOrEmpty(item) || !to.CanAcceptItems(item)) return false;
                
                // if (to.GlobalPosition.X < GlobalPosition.X) to.InsertRight(item);
                // else if (to.GlobalPosition.Y < GlobalPosition.Y) to.InsertRight(item);
                // else to.InsertLeft(item);
                
                // Randomly insert on the left or right
                if (new Random().Next(0, 2) == 0)
                {
                    if (!to.CanInsertLeft(item)) return false;
                    from.Remove(item);
                    to.InsertLeft(item);
                }
                else
                {
                    if (!to.CanInsertRight(item)) return false;
                    from.Remove(item);
                    to.InsertRight(item);
                }

                return true;
                
                // return Inventory.TransferItem(from, to, item);
            }
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
        var sourceEntity = Globals.TileMap.GetEntities(_input, BuildingTileMap.Building);
        return sourceEntity switch
        {
            ITransferTarget building => building.GetFirstItem(),
            _ => null
        };
    }

}