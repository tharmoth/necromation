using System.Collections.Generic;
using Godot;

namespace Necromation.map.character;

public partial class Commander : Node2D, ITransferTarget
{
    public Vector2I MapPosition => MapGlobals.TileMap.GlobalToMap(GlobalPosition);
    public string Name { get; } = MapUtils.GetRandomCommanderName();
    public string Team { get; set; }

    public readonly Inventory Units;
    private readonly Line2D _line = new();
    private readonly Sprite2D _sprite = new();
    
    private Vector2I _targetLocation;
    private Province _province;

    /*
     * RPG Stats
     */
    public int CommandCap = 200;
    public int SquadCap = 3;
    public Vector2I SpawnLocation = new(25, 25);

    public Commander(Province province, string team)
    {
        _province = province;
        _sprite.Texture = GD.Load<Texture2D>("res://res/sprites/player.png");
        _sprite.Scale = new Vector2(0.25f, 0.25f);
        Units = new CommanderInventory(this);
        Team = team;
        
        AddChild(_sprite);
    }
    
    // Deconstructor
    public void Kill()
    {
        if (GetParent() is { } parent) parent.RemoveChild(this);
        _province.Commanders.Remove(this);
        QueueFree();
    }
    
    public override void _Ready()
    {
        base._Ready();
        Globals.MapScene.CallDeferred("add_child", _line);
        
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(_province));
        _targetLocation = MapGlobals.TileMap.GetLocation(_province);
        MapGlobals.TurnListeners.Add(MoveCommander);
        
        if (Team != "Player") _sprite.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        _line.Width = MapGlobals.SelectedCommander == this ? 4 : 8;
        _line.Modulate = MapGlobals.SelectedCommander == this ? Colors.White : new Color(1, 1, 1, 0.5f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if (MapGlobals.SelectedCommander != this) return;

        if (!Input.IsActionJustPressed("right_click")) return;

        _targetLocation = MapGlobals.TileMap.GlobalToMap(GetGlobalMousePosition());
        
        var diff = MapPosition - _targetLocation;
        if (diff.Length() > 1) return;

        var provence = MapGlobals.TileMap.GetProvence(_targetLocation);
        if (provence == null) return;

        _line.Points = new [] {GlobalPosition, MapGlobals.TileMap.MapToGlobal(_targetLocation)};
    }
    
    private void MoveCommander()
    {
        _line.Points = new Vector2[] {};
        if (_targetLocation == MapGlobals.TileMap.GetLocation(_province)) return;
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(_targetLocation);
        _province?.Commanders.Remove(this);
        _province = MapGlobals.TileMap.GetProvence(_targetLocation);
        if (_province == null) return;
        _province.Commanders.Add(this);
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(_province));
    }
    
    private partial class CommanderInventory : Inventory
    {
        private readonly Commander _commander;
        public CommanderInventory(Commander commander) : base()
        {
            _commander = commander;
        }
        
        public override bool CanAcceptItems(string item, int count = 1)
        {
            return CountItem(item) + count <= _commander.CommandCap && base.CanAcceptItems(item, count);
        }
    }
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => Units.CanAcceptItems(item, count);
    public void Insert(string item, int count = 1) => Units.Insert(item, count);
    public bool Remove(string item, int count = 1) => Units.Remove(item, count);
    public string GetFirstItem() => Units.GetFirstItem();
    public List<string> GetItems() => Units.GetItems();
    public List<Inventory> GetInventories() => Units.GetInventories();
    #endregion
}