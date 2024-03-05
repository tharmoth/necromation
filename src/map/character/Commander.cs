using System.Collections.Generic;
using Godot;

namespace Necromation.map.character;

public partial class Commander : Node2D, ITransferTarget
{
    public Vector2I MapPosition => Globals.MapScene.TileMap.GlobalToMap(GlobalPosition);
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
        _sprite.Texture = Database.Instance.GetTexture("Player");
        _sprite.Scale = new Vector2(0.25f, 0.25f);
        Units = new CommanderInventory(this);
        Team = team;
        
        AddChild(_sprite);
    }
    
    // Deconstructor
    public void Kill()
    {
        _province.Commanders.Remove(this);
    }
    
    public override void _Ready()
    {
        base._Ready();
        Globals.MapScene.CallDeferred("add_child", _line);
        
        GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(Globals.MapScene.TileMap.GetLocation(_province));
        _targetLocation = Globals.MapScene.TileMap.GetLocation(_province);
        Globals.MapScene.TurnListeners.Add(MoveCommander);
        
        if (Team != "Player") _sprite.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        _line.Width = Globals.MapScene.SelectedCommander == this ? 4 : 8;
        _line.Modulate = Globals.MapScene.SelectedCommander == this ? Colors.White : new Color(1, 1, 1, 0.5f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if (Globals.MapScene.SelectedCommander != this) return;

        if (!Input.IsActionJustPressed("right_click")) return;

        var newTarget = Globals.MapScene.TileMap.GlobalToMap(GetGlobalMousePosition());
        var diff = MapPosition - newTarget;
        if (diff.Length() > 1.5) return;
        
        var provence = Globals.MapScene.TileMap.GetProvence(newTarget);
        if (provence == null) return;

        _targetLocation = newTarget;
        _line.Points = new [] {GlobalPosition, Globals.MapScene.TileMap.MapToGlobal(_targetLocation)};
    }
    
    private void MoveCommander()
    {
        _line.Points = new Vector2[] {};
        if (_targetLocation == Globals.MapScene.TileMap.GetLocation(_province)) return;
        GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(_targetLocation);
        _province?.Commanders.Remove(this);
        _province = Globals.MapScene.TileMap.GetProvence(_targetLocation);
        if (_province == null) return;
        _province.Commanders.Add(this);
        GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(Globals.MapScene.TileMap.GetLocation(_province));
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