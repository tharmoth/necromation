using System;
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
    public TransportLine LeftLine { get; private set; }
    public TransportLine RightLine { get; private set; }
    
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
    
    // Protected fields
    protected Vector2I MapPosition => Globals.FactoryScene.TileMap.GlobalToMap(GlobalPosition);
    protected virtual Vector2I TargetDirectionGlobal => Orientation switch {
        IRotatable.BuildingOrientation.NorthSouth => new Vector2I(0, -1),
        IRotatable.BuildingOrientation.EastWest => new Vector2I(1, 0),
        IRotatable.BuildingOrientation.SouthNorth => new Vector2I(0, 1),
        IRotatable.BuildingOrientation.WestEast => new Vector2I(-1, 0),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    // Private fields
    private float _secondsPerItem = .5333f;
    private float Speed => FactoryTileMap.TileSize / _secondsPerItem;
    private Vector2I Output => MapPosition + TargetDirectionGlobal;
    private static Vector2I TargetDirectionLocal => new (0, -1);
    private AudioStreamPlayer2D _audio = new();

    
    public Belt(IRotatable.BuildingOrientation orientation)
    {
        _orientation = orientation;

        LeftLine = new TransportLine();
        RightLine = new TransportLine();

        Sprite.Hframes = 8;
        
        // AddChild(_audio);
        _audio.Stream = GD.Load<AudioStream>("res://res/sfx/zapsplat_sport_treadmill_run_fast_no_one_on_22684.mp3");
        _audio.Attenuation = 50.0f;
        _audio.Autoplay = true;
        _audio.VolumeDb = -30.0f;
        _audio.Finished += () => _audio.Play();
    }

    public override void _Ready()
    {
        base._Ready();
        Orientation = _orientation;
        RotationDegrees = IRotatable.GetDegreesFromOrientation(_orientation);
        Sprite.Texture = GD.Load<Texture2D>("res://res/sprites/buildings/BeltAnimated.png");
        // _audio.Play();
        
        LeftLine.TargetDirectionGlobal = TargetDirectionGlobal;
        RightLine.TargetDirectionGlobal = TargetDirectionGlobal;
            
        LeftLine.Init(GlobalPosition + new Vector2(-8, 0).Rotated(GlobalRotation));
        RightLine.Init(GlobalPosition + new Vector2(8, 0).Rotated(GlobalRotation));
        
        // When this belt is placed, update the input and output of all adjacent belts
        GetAdjacent().Values.Where(belt => belt != null).ToList().ForEach(belt => belt.UpdateInputOutput(belt, belt.GetAdjacent()));
        UpdateInputOutput(this, GetAdjacent());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        LeftLine._Process(delta);
        RightLine._Process(delta);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        //
        // LeftLine._Process(delta);
        // RightLine._Process(delta);

        // if (!IsOnScreen) return;
        // // Move the frame forward 8 times every .5333 seconds
        // var seconds = Time.GetTicksMsec() / 1000.0;
        // var frame = (int)(seconds / (_secondsPerItem / 32)) % Sprite.Hframes;
        // if (Sprite.Frame != frame) Sprite.Frame = frame;
    }
    
    public override void Remove(Inventory to)
    {
        var adjacent = GetAdjacent();
        base.Remove(to);
        adjacent.Values.Where(belt => belt != null).ToList().ForEach(belt => UpdateInputOutput(belt, belt.GetAdjacent()));
        UpdateInputOutput(null, adjacent);
    }
    
    /**************************************************************************
     * Protected Overides Methods                                             *
     **************************************************************************/
    public override bool CanPlaceAt(Vector2 position)
    {
        return base.CanPlaceAt(position) 
               || GetOccupiedPositions(position)
                   .Any(pos =>
                   {
                       return Globals.FactoryScene.TileMap.GetEntity(pos, FactoryTileMap.Building) is Belt belt &&
                              belt.Orientation != Orientation;
                   });
    }
    
    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    public void MovePlayer(Character player, double delta)
    {
        if (Globals.FactoryScene.TileMap.GlobalToMap(player.GlobalPosition) != MapPosition) return;
        player.GlobalPosition += -GetTargetLocation(0).DirectionTo(player.GlobalPosition) * Speed * (float)delta;
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
        var global = ToGlobal(direction * FactoryTileMap.TileSize);
        var map = Globals.FactoryScene.TileMap.GlobalToMap(global);
        var entity = Globals.FactoryScene.TileMap.GetEntity(map, FactoryTileMap.Building);

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
        return Globals.FactoryScene.TileMap.GetEntity(Output, FactoryTileMap.Building) as Belt;
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