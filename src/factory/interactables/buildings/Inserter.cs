using System;
using System.Linq;
using Godot;
using Necromation.interactables.belts;
using Necromation.interactables.interfaces;
using Necromation.sk;

namespace Necromation;

public partial class Inserter : Building, IRotatable
{
    /*************************************************************************
     * Data Constants                                                        *
     *************************************************************************/
    // The interval in seconds that the inserter should wait before trying to
    // transfer items again.
    private const double _interval = 0.83;  
    
    /*************************************************************************
     * Public Variables                                                      *
     *************************************************************************/
    // ------------------------- Building Implementation ----------------------
    public override string ItemType => _range == 1 ? "Inserter" : "Long Inserter";
    public override Vector2I BuildingSize => Vector2I.One;
    
    // ------------------------- IRotatable Implementation ---------------------
    public override IRotatable.BuildingOrientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            Sprite.RotationDegrees = IRotatable.GetDegreesFromOrientation(value);
            GhostSprite.RotationDegrees = Sprite.RotationDegrees;
            RotationDegrees = Sprite.RotationDegrees;
        }
    }
    
    /**************************************************************************
     * Private Variables                                                      *
     **************************************************************************/
    // ------------------------- Logic Variables -------------------------------
    // Stores how far the inserter can take and place from in tiles.
    private int _range; 
    // timer that is updated every frame to determine if the inserter should try
    // to transfer.
    private double _time = 0;

    // ------------------------- Visuals Variables -----------------------------
    // Node that plays the inserter sound when it transfers items.
    private AudioStreamPlayer2D _audio = new();
    // Sprite that is used to display the item that the inserter is currently
    // transferring.
    private Sprite2D SpriteInHand = new()
    {
        Visible = false,
        ZIndex = 1
    };
    // Tween that rotates the inserter when it transfers items.
    private Tween _animationTween;
    // The rotation of the inserter. This isn't a node for performance so we'll
    // have to store it manually.
    private float RotationDegrees;
    private float Rotation => Mathf.DegToRad(RotationDegrees);
    // The orientation of the inserter. This is used to determine the input and
    // output positions of the inserter.
    // Can be one of the four cardinal directions.
    private IRotatable.BuildingOrientation _orientation;
    // The input and output tiles for the inserter. These are determined by the
    // orientation of the inserter and the range.
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
    
    // ------------------------- Ugly Performace Caching ----------------------
    // Caches the from and to inventories so we don't need to look them up
    // every frame.
    private ITransferTarget _from;
    private ITransferTarget _to;
    private Belt _belt;

    // Used to cache the results of an insert attempt so we don't need to check
    // every frame unless the inventory changes.
    private bool _hasInventoryChanged = true;
    
    // Caches if the inserter should insert items to the left or right of
    // the belt. This is determined by the
    // orientation of the belt and the inserter.
    private bool _insertLeft;

    /**************************************************************************
     * Constructor                                                            *
     **************************************************************************/
    public Inserter(IRotatable.BuildingOrientation orientation, int range = 1)
    {
        _orientation = orientation;
        
        //TODO: too many hacks to add long inserter. Think about it
        _range = range;
        Sprite.AddChild(SpriteInHand);
        Sprite.ZIndex = 2;
        Sprite.ZAsRelative = false;
        ClipRect.ZIndex = 2;

        _audio.Stream = Database.GetAudio("Inserter");
        _audio.Attenuation = 25.0f;
        _audio.VolumeDb = -20.0f;
        Sprite.AddChild(_audio);
        UpdateInputOutput();
        
        AddComponent(new PowerConsumerComponent(Sprite));
    }
    
    /**************************************************************************
     * Godot Methods                                                          *
     **************************************************************************/
    public override void _Ready()
    {
        base._Ready();

        Orientation = _orientation;
        Globals.FactoryScene.TileMap.listeners.Add(Update);
        Update();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        Globals.FactoryScene.TileMap.listeners.Remove(Update);
        ClearListeners();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        // Make sure we have power before we do anything.
        if (IsTransferring && !GetComponent<PowerConsumerComponent>().DrawPower(delta)) return;
        
        _time += delta;
        SpriteInHand.Visible = IsHoldingItem;

        if (IsTransferring) return;
        
        // Last transfer complete look for new item to transfer
        var item  = Transfer();
            
        // Item found start a transfer
        if (string.IsNullOrEmpty(item)) return;
        Animate(item);
        _time = 0;
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private bool IsTransferring => _time < _interval;
    private bool IsHoldingItem => _time < _interval / 2;
    
    /// <summary>
    /// Caches the from and to inventories so we don't need to look them up.
    /// Also determines if the inserter should insert items to the left or right
    /// of belts.
    /// </summary>
    private void Update()
    {
        // TODO: reenable this performance boost.
        _hasInventoryChanged = true;
        _from = Globals.FactoryScene.TileMap.GetEntity(Input, FactoryTileMap.Building) as ITransferTarget;
        _to = Globals.FactoryScene.TileMap.GetEntity(Output, FactoryTileMap.Building) as ITransferTarget;
        _belt = _to as Belt;
        if (_belt != null)
        {
            _insertLeft = Utils.IsEqualApprox(_belt.RotationDegrees + 90, RotationDegrees)
                          || Utils.IsEqualApprox(_belt.RotationDegrees, RotationDegrees - 90)
                          || Utils.IsEqualApprox(_belt.RotationDegrees, RotationDegrees - 180)
                          || Utils.IsEqualApprox(_belt.RotationDegrees - 270, RotationDegrees)
                          || Utils.IsEqualApprox(_belt.RotationDegrees - 180, RotationDegrees);
        }
        
        ResetListeners();
    }
    
    private void UpdateInputOutput()
    {
        
        var position = -Globals.FactoryScene.TileMap.TileSet.TileSize.ToVector2() * _range * Vector2.Down;
        if (_range > 1)
        {
            position += Globals.FactoryScene.TileMap.TileSet.TileSize.ToVector2() * Vector2.Down / 2.0f;
        }
        SpriteInHand.Position = position;
    }

    private void Animate(string item)
    {
        if (!Config.AnimateInserters || !IsOnScreen) return;
        _audio.Play();
        SpriteInHand.Texture = Database.Instance.GetTexture(item);
        SpriteInHand.Visible = true;
        SpriteInHand.ScaleToSize(Vector2.One * 16);
        
        var target = Mathf.DegToRad(RotationDegrees) + Math.PI;
        var end = Mathf.DegToRad(RotationDegrees);
        var time = _interval / 2;
        
        _animationTween?.Kill();
        _animationTween = Globals.Tree.CreateTween();
        _animationTween.TweenProperty(Sprite, "rotation", target, time);
        _animationTween.TweenProperty(Sprite, "rotation", end, time);
    }
    
    // Attempts to transfer items from the from inventory to the to inventory.
    private string Transfer()
    {
        _hasInventoryChanged = true;
        if (_from == null) return null;
        if (_belt != null)
        {
            foreach (var item in _from.GetItems()
                         .Where(item => !string.IsNullOrEmpty(item) && _to.CanAcceptItems(item)))
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
                if (string.IsNullOrEmpty(item) || !_to.CanAcceptItemsInserter(item)) continue;
                
                var b = Inventory.TransferItem(_from, _to, item);
                Animate(item);
                return item;
            }
        }

        return null;
    }
    
    // Flags the inserter to check if it can transfer items on the next update.
    private void InventoryChanged()
    {
        _hasInventoryChanged = true;
    }

    // Clears then adds most of the inserters associated listeners.
    // This includes the from inventory, and the to inventory.
    // this excludes the tilemap.
    private void ResetListeners()
    {
        ClearListeners();
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

    // Clears the building from and to listeners
    private void ClearListeners()
    {
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