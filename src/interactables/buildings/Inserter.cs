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
    public override IRotatable.BuildingOrientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            RotationDegrees = IRotatable.GetDegreesFromOrientation(value);
        }
    }
    
    private Vector2I position => Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
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

    public Inserter(IRotatable.BuildingOrientation orientation, int range = 1)
    {
        _orientation = orientation;
        
        //TODO: too many hacks to add long inserter. Think about it
        _range = range;
        SetNotifyTransform(true);
        Sprite.AddChild(SpriteInHand);
        Sprite.ZIndex = 2;
        
        AddChild(_audio);
        _audio.Stream = GD.Load<AudioStream>("res://res/sfx/PM_MPRINTER_DPA4060_6_Printer_Printing_Individual_Cycle_Servo_Motor_Toner_Close_Perspectiv_328.mp3");
        _audio.Attenuation = 25.0f;
        _audio.VolumeDb = -10.0f;
        
        Globals.FactoryScene.TileMap.listeners.Add(Update);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Globals.FactoryScene.TileMap.listeners.Remove(Update);
    }

    private ITransferTarget from;
    private ITransferTarget to;
    private Belt belt;
    
    private void Update()
    {
        from = Globals.FactoryScene.TileMap.GetEntity(_input, FactoryTileMap.Building) as ITransferTarget;
        to = Globals.FactoryScene.TileMap.GetEntity(_output, FactoryTileMap.Building) as ITransferTarget;
        belt = to as Belt;
    }

    public override void _Ready()
    {
        base._Ready();
        Orientation = _orientation;
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
            -FactoryTileMap.TileSize * _range * new Vector2I(0, 1) 
            : -FactoryTileMap.TileSize * _range * new Vector2I(0, 1) + FactoryTileMap.TileSize * new Vector2I(0, 1) / 2;
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
        if (IsOnScreen) return;
        SpriteInHand.Texture =  Database.Instance.GetTexture(item);
        SpriteInHand.Visible = true;
        SpriteInHand.Scale = new Vector2(16 / SpriteInHand.Texture.GetSize().X, 16 / SpriteInHand.Texture.GetSize().Y);
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
        if (from == null) return false;
        if (belt != null)
        {
            foreach (var item in from.GetItems().Where(item => !string.IsNullOrEmpty(item) && to.CanAcceptItems(item)))
            {
                // Grab these rotations to avoid GODOT system calls.
                var beltRotation = belt.RotationDegrees;
                var inserterRotation = RotationDegrees;
                // Weird double checks due to degree weirdness. Should probably be done another way.
                // BAD BAD BAD CODE - USE BRAIN TO MAKE BETTER
                if ((TransportLine.IsEqualApprox(beltRotation + 90, inserterRotation)
                     || TransportLine.IsEqualApprox(beltRotation, inserterRotation - 90)
                     || TransportLine.IsEqualApprox(beltRotation, inserterRotation - 180)
                     || TransportLine.IsEqualApprox(beltRotation - 270, inserterRotation)
                     || TransportLine.IsEqualApprox(beltRotation - 180, inserterRotation)) &&
                    belt.CanInsertLeft(item))
                {
                    from.Remove(item);
                    belt.InsertLeft(item);
                    Animate(item);
                    return true;
                }

                if ((TransportLine.IsEqualApprox(beltRotation - 90, inserterRotation)
                     || TransportLine.IsEqualApprox(beltRotation, inserterRotation + 90)
                     || TransportLine.IsEqualApprox(beltRotation, inserterRotation - 270)
                     || TransportLine.IsEqualApprox(beltRotation, inserterRotation)) && belt.CanInsertRight(item))
                {
                    from.Remove(item);
                    belt.InsertRight(item);
                    Animate(item);
                    return true;
                }
            }
        } 
        else if (to != null)
        {
            foreach (var item in from.GetItems())
            {
                if (string.IsNullOrEmpty(item) || !to.CanAcceptItems(item)) continue;
                
                var b = Inventory.TransferItem(from, to, item);
                Animate(item);
                return b;
            }
        }

        return false;
    }

    private string GetSourceItem()
    {
        var sourceEntity = Globals.FactoryScene.TileMap.GetEntity(_input, FactoryTileMap.Building);
        return sourceEntity switch
        {
            ITransferTarget building => building.GetFirstItem(),
            _ => null
        };
    }

}