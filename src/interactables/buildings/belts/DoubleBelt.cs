using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class DoubleBelt : Building, ITransferTarget, IRotatable
{
    private float _secondsPerItem = .5333f;
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Double Belt";

    private TransportLine _leftLine = new();
    private TransportLine _rightLine = new();
    
    public TransportLine LeftLine => _leftLine;
    public TransportLine RightLine => _rightLine;
    
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
    
    protected virtual Vector2 GetTargetLocation(int index)
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
        
        // When this belt is placed, update the input and output of all adjacent belts
        GetAdjacent().Values.Where(belt => belt != null).ToList().ForEach(belt => belt.UpdateInputOutput(belt, belt.GetAdjacent()));
        UpdateInputOutput(this, GetAdjacent());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MovePlayer(delta);
    }
    
    protected override void Remove(Inventory to)
    {
        var adjacent = GetAdjacent();
        base.Remove(to);
        adjacent.Values.Where(belt => belt != null).ToList().ForEach(belt => UpdateInputOutput(belt, belt.GetAdjacent()));
        UpdateInputOutput(null, adjacent);
    }
    
    private DoubleBelt GetBeltInDirection(Vector2 direction)
    {
        var global = ToGlobal(direction * BuildingTileMap.TileSize);
        var map = Globals.TileMap.GlobalToMap(global);
        var entity = Globals.TileMap.GetEntities(map, BuildingTileMap.Building);

        return entity is DoubleBelt belt && (belt.GetOutputBelt() == this || belt == GetOutputBelt()) ? belt : null;
    }
    
    /// <summary>
    /// Returns a dictionary of adjacent belts. If no belt is found in a direction, the corresponding dictionary value
    /// will be null.
    /// </summary>
    /// <returns>A dictionary with keys {"Forward", "Behind", "Right", "Left"} and values as the corresponding
    /// adjacent <see cref="DoubleBelt"/> instances or null if no belt is present in the direction.</returns>
    private Dictionary<string, DoubleBelt> GetAdjacent()
    {
        var rotatedRight = ((Vector2)TargetDirectionLocal).Rotated(Mathf.DegToRad(90)).Snapped(Vector2.One);
        var rotatedLeft = ((Vector2)TargetDirectionLocal).Rotated(Mathf.DegToRad(-90)).Snapped(Vector2.One);

        var outputBelt = GetOutputBelt();
        var beltBehind = GetBehindBelt();
        var beltRight = GetBeltInDirection(rotatedRight);
        var beltLeft = GetBeltInDirection(rotatedLeft);

        return new Dictionary<string, DoubleBelt>
        {
            { "Output", outputBelt },
            { "Behind", beltBehind },
            { "Right", beltRight },
            { "Left", beltLeft }
        };
    }

    protected virtual void UpdateInputOutput(DoubleBelt belt, Dictionary<string, DoubleBelt> belts)
    {
        // There are 5 cases to consider
        // 1. If a behind belt inputs to this one link left and right
        // 2. If a left belt inputs to this one and there are no other inputs link left and right.
        // 3. if a left belt inputs to this one and there is another input link only left
        // 4. if a right belt inputs to this one and there are no other inputs link left and right.
        // 5. if a right belt inputs to this one and there is another input link only right
        var beltBehind = belts["Behind"];
        var beltRight = belts["Right"];
        var beltLeft = belts["Left"];
        
        var leftLine = belt?._leftLine;
        var rightLine = belt?._rightLine;

        if (beltBehind != null)
        {
            beltBehind._leftLine.OutputLine = leftLine;
            beltBehind._rightLine.OutputLine = rightLine;
        }
        
        if (beltLeft != null && beltRight == null && beltBehind == null)
        {
            beltLeft._leftLine.OutputLine = leftLine;
            beltLeft._rightLine.OutputLine = rightLine;
        }
        else if (beltLeft != null)
        {
            beltLeft._leftLine.OutputLine = leftLine;
            beltLeft._rightLine.OutputLine = leftLine;
        }
        
        if (beltRight != null && beltLeft == null && beltBehind == null)
        {
            beltRight._leftLine.OutputLine = leftLine;
            beltRight._rightLine.OutputLine = rightLine;
        }
        else if (beltRight != null)
        {
            beltRight._leftLine.OutputLine = rightLine;
            beltRight._rightLine.OutputLine = rightLine;
        }
    }

    private void MovePlayer(double delta)
    {
        if (Globals.TileMap.GlobalToMap(Globals.Player.GlobalPosition) != MapPosition) return;
        Globals.Player.GlobalPosition += -GetTargetLocation(0).DirectionTo(Globals.Player.GlobalPosition) * Speed * (float)delta;
    }
    
    protected virtual DoubleBelt GetOutputBelt()
    {
        return Globals.TileMap.GetEntities(Output, BuildingTileMap.Building) as DoubleBelt;
    }
    
    protected virtual DoubleBelt GetBehindBelt()
    {
        return GetBeltInDirection(-TargetDirectionLocal);
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
    
    public List<string> GetItems()
    {
        var items = _leftLine.GetItems().ToList();
        items.AddRange(_rightLine.GetItems());
        return items;
    }
    
    public List<Inventory> GetInventories() => new()
        { _leftLine.GetInventories().First(), _rightLine.GetInventories().First() };

    #endregion



}