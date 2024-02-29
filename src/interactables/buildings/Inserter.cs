using System;
using System.Linq;
using Godot;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;

namespace Necromation;

public partial class Inserter : Building, IRotatable
{
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => _range == 1 ? "Inserter" : "Long Inserter";
    private double _time = 0;
    private double _interval = .83;
    private Tween _tween;
    private int _range;
    private Sprite2D SpriteInHand = new()
    {
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
    private Vector2I _input => position + _range * Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    private Vector2I _output => position + _range * Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(-1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    private AudioStreamPlayer2D _audio = new();

    public Inserter(int range = 1)
    {
        //TODO: too many hacks to add long inserter. Think about it
        _range = range;
        SetNotifyTransform(true);
        Sprite.AddChild(SpriteInHand);
        Sprite.ZIndex = 2;
        
        AddChild(_audio);
        _audio.Stream = GD.Load<AudioStream>("res://res/sfx/PM_MPRINTER_DPA4060_6_Printer_Printing_Individual_Cycle_Servo_Motor_Toner_Close_Perspectiv_328.mp3");
        _audio.Attenuation = 25.0f;
        _audio.VolumeDb = -10.0f;
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what != NotificationTransformChanged) return;
        UpdateInputOutput();
    }
    
    private void UpdateInputOutput()
    {
        SpriteInHand.Position = _range == 1 ? 
            -BuildingTileMap.TileSize * _range * new Vector2I(0, 1) 
            : -BuildingTileMap.TileSize * _range * new Vector2I(0, 1) + BuildingTileMap.TileSize * new Vector2I(0, 1) / 2;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _time += delta;
        if (_time < _interval) return;

        var item = GetSourceItem();
        if (!Transfer()) return;
        _audio.Play();
        _time = 0;
    }

    private void Animate(string item)
    {
        SpriteInHand.Texture =  Globals.Database.GetTexture(item);
        SpriteInHand.Visible = true;
        SpriteInHand.Scale = new Vector2(16 / SpriteInHand.Texture.GetSize().X, 16 / SpriteInHand.Texture.GetSize().Y);
        GD.Print(Rotation + " " + (Rotation + Math.PI));
        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(Sprite, "rotation", Math.PI, _interval/2);
        _tween.TweenCallback(new Callable(this, "DropItem"));
        _tween.TweenProperty(Sprite, "rotation", 0, _interval/2);
    }

    private void DropItem()
    {
        SpriteInHand.Visible = false;
    }

    private bool Transfer()
    {
        var sourceEntity = Globals.TileMap.GetEntity(_input, BuildingTileMap.Building);
        var targetEntity = Globals.TileMap.GetEntity(_output, BuildingTileMap.Building);
        
        switch (sourceEntity, targetEntity)
        {
            case (ITransferTarget from, Belt to):
            {
                foreach (var item in from.GetItems().Where(item => !string.IsNullOrEmpty(item) && to.CanAcceptItems(item)))
                {
                    // Weird double checks due to degree weirdness. Should probably be done another way.
                    // BAD BAD BAD CODE - USE BRAIN TO MAKE BETTER
                    if ((TransportLine.IsEqualApprox(to.RotationDegrees + 90, RotationDegrees) 
                         || TransportLine.IsEqualApprox(to.RotationDegrees, RotationDegrees - 90) 
                         || TransportLine.IsEqualApprox(to.RotationDegrees, RotationDegrees - 180) 
                         || TransportLine.IsEqualApprox(to.RotationDegrees - 270, RotationDegrees) 
                         || TransportLine.IsEqualApprox(to.RotationDegrees - 180, RotationDegrees)) && to.CanInsertLeft(item))
                    {
                        from.Remove(item);
                        to.InsertLeft(item);
                        Animate(item);
                        return true;
                    }
                    if ((TransportLine.IsEqualApprox(to.RotationDegrees - 90, RotationDegrees) 
                         || TransportLine.IsEqualApprox(to.RotationDegrees, RotationDegrees + 90)
                         || TransportLine.IsEqualApprox(to.RotationDegrees, RotationDegrees - 270)
                         || TransportLine.IsEqualApprox(to.RotationDegrees, RotationDegrees)) && to.CanInsertRight(item))
                    {
                        from.Remove(item);
                        to.InsertRight(item);
                        Animate(item);
                        return true;
                    }
                    // GD.Print("Belt ROtation: " + to.RotationDegrees + " Inserter Rotation: " + RotationDegrees);
                }
                return false;
            }
            case (ITransferTarget from, ITransferTarget to):
            {
                foreach (var item in from.GetItems())
                {
                    if (string.IsNullOrEmpty(item) || !to.CanAcceptItems(item)) continue;
                    
                    var b = Inventory.TransferItem(from, to, item);
                    Animate(item);
                    return b;
                }

                return false;
            }
        }
        return false;
    }

    private string GetSourceItem()
    {
        var sourceEntity = Globals.TileMap.GetEntity(_input, BuildingTileMap.Building);
        return sourceEntity switch
        {
            ITransferTarget building => building.GetFirstItem(),
            _ => null
        };
    }

}