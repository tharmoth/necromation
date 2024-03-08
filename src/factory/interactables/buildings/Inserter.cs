﻿using System;
using System.Linq;
using Godot;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;

namespace Necromation;

public partial class Inserter : Building, IRotatable
{
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => _range == 1 ? "Inserter" : "Long Inserter";
    
    // Stores how far the inserter can take and place from in tiles.
    private int _range;
    
    // timer that is updated every frame to determine if the inserter should try to transfer.
    private double _time = 0;
    
    // The interval in seconds that the inserter should wait before trying to transfer items again.
    private double _interval = .83;
    
    /**************************************************************************
     * Child Nodes                                                            *
     **************************************************************************/
    // Node that plays the inserter sound when it transfers items.
    private AudioStreamPlayer2D _audio = new();
    
    // Sprite of the item in teh inserters hand. Is Invisible when an inserter isn't moving the hand towards the destination.
    private Sprite2D SpriteInHand = new()
    {
        Visible = false,
        ZIndex = 1
    };
    
    // Tween that rotates the inserter when it transfers items.
    private Tween _rotationTween;
    
    // The orientation of the inserter. This is used to determine the input and output positions of the inserter.
    // Can be one of the four cardinal directions.
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
    
    // The input and output tiles for the inserter. These are determined by the orientation of the inserter and the range.
    private Vector2I Input => MapPosition + _range * Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    private Vector2I Output => MapPosition + _range * Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(-1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    /**************************************************************************
     * Ugly Performace Caching                                                *
     **************************************************************************/
    // Caches the from and to inventories so we don't need to look them up every frame.
    private ITransferTarget _from;
    private ITransferTarget _to;
    private Belt _belt;
    
    // Stores the current state of the inserter. If true, the inserter is currently transferring items. needed to drop
    // the items if the Tween isn't running.
    private bool _isTransferring = false;
    
    // Used to cache the results of an insert attempt so we don't need to check every frame unless the inventory changes.
    private bool _hasInventoryChanged = true;
    
    // Caches weather the inserter should insert items to the left or right of the belt. This is determined by the
    // orientation of the belt and the inserter.
    private bool _insertLeft;

    public Inserter(IRotatable.BuildingOrientation orientation, int range = 1)
    {
        _orientation = orientation;
        
        //TODO: too many hacks to add long inserter. Think about it
        _range = range;
        SetNotifyTransform(true);
        Sprite.AddChild(SpriteInHand);
        Sprite.ZIndex = 2;
        
        _audio.Stream = GD.Load<AudioStream>("res://res/sfx/PM_MPRINTER_DPA4060_6_Printer_Printing_Individual_Cycle_Servo_Motor_Toner_Close_Perspectiv_328.mp3");
        _audio.Attenuation = 25.0f;
        _audio.VolumeDb = -10.0f;
        AddChild(_audio);
        
        Globals.FactoryScene.TileMap.listeners.Add(Update);
    }

    private void Update()
    {
        _from = Globals.FactoryScene.TileMap.GetEntity(Input, FactoryTileMap.Building) as ITransferTarget;
        _to = Globals.FactoryScene.TileMap.GetEntity(Output, FactoryTileMap.Building) as ITransferTarget;
        _belt = _to as Belt;
        if (_belt != null)
        {
            _insertLeft = TransportLine.IsEqualApprox(_belt.RotationDegrees + 90, RotationDegrees)
                          || TransportLine.IsEqualApprox(_belt.RotationDegrees, RotationDegrees - 90)
                          || TransportLine.IsEqualApprox(_belt.RotationDegrees, RotationDegrees - 180)
                          || TransportLine.IsEqualApprox(_belt.RotationDegrees - 270, RotationDegrees)
                          || TransportLine.IsEqualApprox(_belt.RotationDegrees - 180, RotationDegrees);
        }

        CallDeferred("ResetListeners");
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
        if (_time > _interval / 2 && _isTransferring)
        {
            _isTransferring = false;
            DropItem();
            return;
        }
        if (_time < _interval || !_hasInventoryChanged) return;
        
        var item  = Transfer();
        _isTransferring = !string.IsNullOrEmpty(item);
        
        if (!_isTransferring) return;
        Animate(item);
        _time = 0;
    }

    private void Animate(string item)
    {
        if (!Config.AnimateInserters || !IsOnScreen) return;
        _audio.Play();
        SpriteInHand.Texture =  Database.Instance.GetTexture(item);
        SpriteInHand.Visible = true;
        SpriteInHand.Scale = new Vector2(16 / SpriteInHand.Texture.GetSize().X, 16 / SpriteInHand.Texture.GetSize().Y);
        _rotationTween?.Kill();
        _rotationTween = GetTree().CreateTween();
        _rotationTween.TweenProperty(Sprite, "rotation", Math.PI, _interval/2);
        _rotationTween.TweenProperty(Sprite, "rotation", 0, _interval/2);
    }
    
    // Attempts to transfer items from the from inventory to the to inventory.
    private string Transfer()
    {
        _hasInventoryChanged = false;
        if (_from == null) return null;
        if (_belt != null)
        {
            foreach (var item in _from.GetItems().Where(item => !string.IsNullOrEmpty(item) && _to.CanAcceptItems(item)))
            {
                switch (_insertLeft)
                {
                    case true when _belt.CanInsertLeft(item):
                        _from.Remove(item);
                        _belt.InsertLeft(item);
                        Animate(item);
                        return item;
                    case false when _belt.CanInsertRight(item):
                        _from.Remove(item);
                        _belt.InsertRight(item);
                        Animate(item);
                        return item;
                }
            }
        } 
        else if (_to != null)
        {
             foreach (var item in _from.GetItems())
            {
                if (string.IsNullOrEmpty(item) || !_to.CanAcceptItems(item)) continue;
                
                var b = Inventory.TransferItem(_from, _to, item);
                Animate(item);
                return item;
            }
        }

        return null;
    }
    
    // Hides the sprite in the inserters hand.
    private void DropItem()
    {
        SpriteInHand.Visible = false;
    }
    
    // Flags the inserter to check if it can transfer items on the next update.
    private void InventoryChanged()
    {
        _hasInventoryChanged = true;
    }

    // Called when the node is removed or QueueFreed. Clears all of the inserters associated listeners.
    public override void _ExitTree()
    {
        base._ExitTree();
        
        ClearListeners();
    }
    
    // Clears then adds all of the inserters associated listeners.
    // This includes the tilemap, the from inventory, and the to inventory.
    private void ResetListeners()
    {
        ClearListeners();
        Globals.FactoryScene.TileMap.listeners.Remove(Update);
        _from?.GetOutputInventory().Listeners.Add(InventoryChanged);
        if (_belt != null)
        {
            _belt.LeftLine.GetInventories().First().Listeners.Add(InventoryChanged);
            _belt.RightLine.GetInventories().First().Listeners.Add(InventoryChanged);
        }
        else
        {
            _to?.GetInputInventory().Listeners.Add(InventoryChanged);
        }
    }

    // Clears all of the inserters associated listeners. Needs to be called before this node is freed or we'll get
    // instanceremoved errors... probably.
    private void ClearListeners()
    {
        Globals.FactoryScene.TileMap.listeners.Remove(Update);
        _from?.GetOutputInventory().Listeners.Remove(InventoryChanged);
        if (_belt != null)
        {
            _belt.LeftLine.GetInventories().First().Listeners.Remove(InventoryChanged);
            _belt.RightLine.GetInventories().First().Listeners.Remove(InventoryChanged);
        }
        else
        {
            _to?.GetInputInventory().Listeners.Remove(InventoryChanged);
        }
    }
}