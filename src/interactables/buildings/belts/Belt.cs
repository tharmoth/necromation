﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.interactables.belts;

public partial class Belt : Building, ITransferTarget, IRotatable
{
    // Public fields
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Belt";
    public TransportLine LeftLine { get; private set; } = new();
    public TransportLine RightLine { get; private set; } = new();
    
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
    
    // Protected fields
    protected Vector2I MapPosition => Globals.TileMap.GlobalToMap(GlobalPosition);
    protected virtual Vector2I TargetDirectionGlobal => Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    // Private fields
    private float _secondsPerItem = .5333f;
    private float Speed => BuildingTileMap.TileSize / _secondsPerItem;
    private Vector2I Output => MapPosition + TargetDirectionGlobal;
    private static Vector2I TargetDirectionLocal => new (0, -1);

    public Belt()
    {
        AddChild(LeftLine);
        AddChild(RightLine);
        LeftLine.Position = new Vector2(-8, 0);
        RightLine.Position = new Vector2(8, 0);
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
    
    /**************************************************************************
     * Protected Overides Methods                                             *
     **************************************************************************/
    protected override void Remove(Inventory to)
    {
        var adjacent = GetAdjacent();
        base.Remove(to);
        adjacent.Values.Where(belt => belt != null).ToList().ForEach(belt => UpdateInputOutput(belt, belt.GetAdjacent()));
        UpdateInputOutput(null, adjacent);
    }
    
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) 
               || GetOccupiedPositions(position)
                   .Any(pos => Globals.TileMap.GetEntities(pos, BuildingTileMap.Building) is Belt belt && belt.Orientation != Orientation);
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private void MovePlayer(double delta)
    {
        if (Globals.TileMap.GlobalToMap(Globals.Player.GlobalPosition) != MapPosition) return;
        Globals.Player.GlobalPosition += -GetTargetLocation(0).DirectionTo(Globals.Player.GlobalPosition) * Speed * (float)delta;
    }
    
    private Vector2 GetTargetLocation(int index)
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

    #region StrangeBeltLogic
    /**************************************************************************
     * Strange Belt Logic                                                     *
     **************************************************************************/
    private Dictionary<string, Belt> GetAdjacent()
    {
        var rotatedRight = ((Vector2)TargetDirectionLocal).Rotated(Mathf.DegToRad(90)).Snapped(Vector2.One);
        var rotatedLeft = ((Vector2)TargetDirectionLocal).Rotated(Mathf.DegToRad(-90)).Snapped(Vector2.One);

        var outputBelt = GetOutputBelt();
        var beltBehind = GetBehindBelt();
        var beltRight = GetBeltInDirection(rotatedRight);
        var beltLeft = GetBeltInDirection(rotatedLeft);

        return new Dictionary<string, Belt>
        {
            { "Output", outputBelt },
            { "Behind", beltBehind },
            { "Right", beltRight },
            { "Left", beltLeft }
        };
    }
    
    private Belt GetBeltInDirection(Vector2 direction)
    {
        var global = ToGlobal(direction * BuildingTileMap.TileSize);
        var map = Globals.TileMap.GlobalToMap(global);
        var entity = Globals.TileMap.GetEntities(map, BuildingTileMap.Building);

        return entity is Belt belt && (belt.GetOutputBelt() == this || belt == GetOutputBelt()) ? belt : null;
    }

    protected virtual void UpdateInputOutput(Belt belt, Dictionary<string, Belt> belts)
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
        
        var leftLine = belt?.LeftLine;
        var rightLine = belt?.RightLine;

        if (beltBehind != null)
        {
            beltBehind.LeftLine.OutputLine = leftLine;
            beltBehind.RightLine.OutputLine = rightLine;
        }
        
        if (beltLeft != null && beltRight == null && beltBehind == null)
        {
            beltLeft.LeftLine.OutputLine = leftLine;
            beltLeft.RightLine.OutputLine = rightLine;
        }
        else if (beltLeft != null)
        {
            beltLeft.LeftLine.OutputLine = leftLine;
            beltLeft.RightLine.OutputLine = leftLine;
        }
        
        if (beltRight != null && beltLeft == null && beltBehind == null)
        {
            beltRight.LeftLine.OutputLine = leftLine;
            beltRight.RightLine.OutputLine = rightLine;
        }
        else if (beltRight != null)
        {
            beltRight.LeftLine.OutputLine = rightLine;
            beltRight.RightLine.OutputLine = rightLine;
        }
    }

    protected virtual Belt GetOutputBelt()
    {
        return Globals.TileMap.GetEntities(Output, BuildingTileMap.Building) as Belt;
    }
    
    protected virtual Belt GetBehindBelt()
    {
        return GetBeltInDirection(-TargetDirectionLocal);
    }

    public void InsertLeft(string item, int count = 1)
    {
        if (LeftLine.GetItemCount() + count < 5) LeftLine.Insert(item, count);
    }

    public void InsertRight(string item, int count = 1)
    {
        if (RightLine.GetItemCount() + count < 5) RightLine.Insert(item, count);
    }
    
    public bool CanInsertLeft(string item, int count = 1)
    {
        return LeftLine.GetItemCount() + count < 5;
    }
    
    public bool CanInsertRight(string item, int count = 1)
    {
        return RightLine.GetItemCount() + count < 5;
    }
    #endregion

    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item,  int count = 1)
    {
        return LeftLine.GetItemCount() + count < 5 || RightLine.GetItemCount() + count < 5;
    }
    
    public void Insert(string item, int count = 1)
    { 
        // if the position is to the left of the center of the building, insert into the left belt
        if (LeftLine.GetItemCount() + count < 5) LeftLine.Insert(item, count);
        else if (RightLine.GetItemCount() + count < 5) RightLine.Insert(item, count);
    }
    
    public bool Remove(string item, int count = 1)
    {
        if (LeftLine.GetItemCount(item) >= count) return LeftLine.Remove(item, count);
        if (RightLine.GetItemCount(item) >= count) return RightLine.Remove(item, count);
        return false;
    }
    
    public string GetFirstItem()
    {
        var item = LeftLine.GetInventories().First().GetFirstItem();
        item ??= RightLine.GetInventories().First().GetFirstItem();;
        return item;
    }
    
    public List<string> GetItems()
    {
        var items = LeftLine.GetItems().ToList();
        items.AddRange(RightLine.GetItems());
        return items;
    }
    
    public List<Inventory> GetInventories() => new()
        { LeftLine.GetInventories().First(), RightLine.GetInventories().First() };

    #endregion
}