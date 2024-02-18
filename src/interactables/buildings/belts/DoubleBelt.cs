using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class DoubleBelt : Building, ITransferTarget, IRotatable
{
    protected float _secondsPerItem = .5333f;
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Double Belt";

    private TransportLine _leftLine = new();
    private TransportLine _rightLine = new();
    
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
    protected float Speed => BuildingTileMap.TileSize / _secondsPerItem;
    private Vector2I Output => MapPosition + TargetDirectionGlobal;
    protected Vector2I MapPosition => Globals.TileMap.GlobalToMap(GlobalPosition);
    protected virtual Vector2I TargetDirectionGlobal => Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    protected virtual Vector2I TargetDirectionLocal => new (0, -1);
    
    protected Vector2 GetTargetLocation(int index)
    {
        return index switch
        {
            0 => GlobalPosition + TargetDirectionGlobal * 16,
            1 => GlobalPosition + TargetDirectionGlobal * 8,
            2 => GlobalPosition - TargetDirectionGlobal * 0,
            3 => GlobalPosition - TargetDirectionGlobal * 8,
            4 => GlobalPosition - TargetDirectionGlobal * 16,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };
    }

    public DoubleBelt()
    {
        AddChild(_leftLine);
        AddChild(_rightLine);
        _leftLine.Position = new Vector2(-8, 0);
        _rightLine.Position = new Vector2(8, 0);
    }

    public override void _Ready()
    {
        base._Ready();
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MovePlayer(delta);

        // TODO: only call this when needed!
        SetInputOutput();
    }

    /*
     * Returns a list of adjacent belts. If there is no belt in a given direction, the value will be null
     * The order of the list is as follows:
     * 0: Forward
     * 1: Behind
     * 2: Right
     * 3: Left
     */
    private List<DoubleBelt> GetAdjacent()
    {
        var rotatedRight = (Vector2) TargetDirectionLocal;
        rotatedRight = (Vector2I) rotatedRight.Rotated(Mathf.DegToRad(90)).Snapped(Vector2.One);

        var rotatedLeft = (Vector2) TargetDirectionLocal;
        rotatedLeft = (Vector2I) rotatedLeft.Rotated(Mathf.DegToRad(-90)).Snapped(Vector2.One);

        var forwardGlobal = ToGlobal(TargetDirectionLocal * BuildingTileMap.TileSize);
        var behindGlobal = ToGlobal(-TargetDirectionLocal * BuildingTileMap.TileSize);
        var rightGlobal = ToGlobal(rotatedRight * BuildingTileMap.TileSize);
        var leftGlobal = ToGlobal(rotatedLeft * BuildingTileMap.TileSize);
        
        var forwardMap = Globals.TileMap.GlobalToMap(forwardGlobal);
        var behindMap = Globals.TileMap.GlobalToMap(behindGlobal);
        var rightMap = Globals.TileMap.GlobalToMap(rightGlobal);
        var leftMap = Globals.TileMap.GlobalToMap(leftGlobal);
        
        var entityForward = Globals.TileMap.GetEntities(forwardMap, BuildingTileMap.LayerNames.Buildings);
        var entityBehind = Globals.TileMap.GetEntities(behindMap, BuildingTileMap.LayerNames.Buildings);
        var entityRight = Globals.TileMap.GetEntities(rightMap, BuildingTileMap.LayerNames.Buildings);
        var entityLeft = Globals.TileMap.GetEntities(leftMap, BuildingTileMap.LayerNames.Buildings);

        var beltForward = entityForward is DoubleBelt belt0 && belt0.GetNextBelt() == this ? belt0 : null;
        var beltBehind = entityBehind is DoubleBelt belt1 && belt1.GetNextBelt() == this ? belt1 : null;
        var beltRight = entityRight is DoubleBelt belt2 && belt2.GetNextBelt() == this ? belt2 : null;
        var beltLeft = entityLeft is DoubleBelt belt3 && belt3.GetNextBelt() == this ? belt3 : null;

        return new List<DoubleBelt>() { beltForward, beltBehind, beltRight, beltLeft };
    }

    private void SetInputOutput()
    {
        // There are 5 cases to consider
        // 1. If a behind belt inputs to this one link left and right
        // 2. If a left belt inputs to this one and there are no other inputs link left and right.
        // 3. if a left belt inputs to this one and there is another input link only left
        // 4. if a right belt inputs to this one and there are no other inputs link left and right.
        // 5. if a right belt inputs to this one and there is another input link only right

        var belts = GetAdjacent();
        var beltForward = belts[0];
        var beltBehind = belts[1];
        var beltRight = belts[2];
        var beltLeft = belts[3];

        if (beltBehind != null)
        {
            beltBehind._leftLine.OutputLine = _leftLine;
            beltBehind._rightLine.OutputLine = _rightLine;
        }
        
        if (beltLeft != null && beltRight == null && beltForward == null && beltBehind == null)
        {
            beltLeft._leftLine.OutputLine = _leftLine;
            beltLeft._rightLine.OutputLine = _rightLine;
        }
        else if (beltLeft != null)
        {
            beltLeft._leftLine.OutputLine = _leftLine;
            beltLeft._rightLine.OutputLine = _leftLine;
        }
        
        if (beltRight != null && beltLeft == null && beltForward == null && beltBehind == null)
        {
            beltRight._leftLine.OutputLine = _leftLine;
            beltRight._rightLine.OutputLine = _rightLine;
        }
        else if (beltRight != null)
        {
            beltRight._leftLine.OutputLine = _rightLine;
            beltRight._rightLine.OutputLine = _rightLine;
        }
    }

    private void MovePlayer(double delta)
    {
        if (Globals.TileMap.GlobalToMap(Globals.Player.GlobalPosition) != MapPosition) return;
        Globals.Player.GlobalPosition += -GetTargetLocation(0).DirectionTo(Globals.Player.GlobalPosition) * Speed * (float)delta;
    }
    
    protected virtual BuildingTileMap.IEntity GetNextBelt()
    {
        return Globals.TileMap.GetEntities(Output, BuildingTileMap.LayerNames.Buildings);
    }
    
    public TransportLine GetLeftInventory() => _leftLine;
    public TransportLine GetRightInventory() => _rightLine;
    
    public void InsertLeft(string item, int count = 1)
    {
        if (_leftLine.GetItemCount() + count < 5) _leftLine.Insert(item, count);
    }

    public void InsertRight(string item, int count = 1)
    {
        if (_rightLine.GetItemCount() + count < 5) _rightLine.Insert(item, count);
    }
    
    public bool CanInsertLeft(string item, int count = 1)
    {
        return _leftLine.GetItemCount() + count < 5;
    }
    
    public bool CanInsertRight(string item, int count = 1)
    {
        return _rightLine.GetItemCount() + count < 5;
    }

    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item,  int count = 1)
    {
        return _leftLine.GetItemCount() + count < 5 || _rightLine.GetItemCount() + count < 5;
    }
    
    public void Insert(string item, int count = 1)
    { 
        // if the position is to the left of the center of the building, insert into the left belt
        if (_leftLine.GetItemCount() + count < 5) _leftLine.Insert(item, count);
        else if (_rightLine.GetItemCount() + count < 5) _rightLine.Insert(item, count);
    }
    
    public bool Remove(string item, int count = 1)
    {
        if (_leftLine.GetItemCount(item) >= count) return _leftLine.Remove(item, count);
        if (_rightLine.GetItemCount(item) >= count) return _rightLine.Remove(item, count);
        return false;
    }
    
    public string GetFirstItem()
    {
        var item = _leftLine.GetInventories().First().GetFirstItem();
        item ??= _rightLine.GetInventories().First().GetFirstItem();;
        return item;
    }
    
    public List<Inventory> GetInventories() => new()
        { _leftLine.GetInventories().First(), _rightLine.GetInventories().First() };

    #endregion



}