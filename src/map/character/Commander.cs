using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.map.character;

public partial class Commander : Node2D, ITransferTarget
{
    /**************************************************************************
     * Utility Property                                                       *
     **************************************************************************/
    public Vector2I MapPosition => Globals.MapScene.TileMap.GlobalToMap(GlobalPosition);

    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    // Note: If you add more state data here make sure to serialize it in Save/Load
    public readonly string CommanderName;
    public readonly string Team;
    public readonly Inventory Units;
    private Province _province;
    private Vector2I _targetLocation;
    public Vector2I SpawnLocation = new(25, 25);
    /// <summary> Associates this commander with a barracks in the factory. Used for save/load. </summary>
    public string BarracksId = null;
    
    public Command CurrentCommand = Command.None;
    public enum Command
    {
        None, HoldAndAttack, Fire, HoldAndFire, FireAndKeepDistance
    }
    
    public Target TargetType = Target.Closest;
    public enum Target
    {
        Closest, Archers, Cavalry, Rearmost, Random
    }

    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private readonly Line2D _line = new();
    private readonly Sprite2D _sprite = new();
    
    /**************************************************************************
     * RPG Constants     													  *
     **************************************************************************/
    public int CommandCap = 200;
    
    public Commander(Province province, string team, string name = null)
    {
        _province = province;
        _sprite.Texture = Database.Instance.GetTexture("Player");
        _sprite.Scale = new Vector2(0.25f, 0.25f);
        Units = new CommanderInventory(this);
        Team = team;
        CommanderName = name ?? MapUtils.GetRandomCommanderName();
        
        AddChild(_sprite);
        
        province.AddCommander(this);
    }
    
    // Deconstructor
    public void Kill()
    {
        _province.Commanders.Remove(this);
        QueueFree();
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
        
        _line.Width = Globals.MapScene.SelectedCommanders.Contains(this) ? 4 : 8;
        _line.Modulate = Globals.MapScene.SelectedCommanders.Contains(this) ? Colors.White : new Color(.56f, .56f, .56f, 0.25f);
        _line.ZIndex = Globals.MapScene.SelectedCommanders.Contains(this) ? 2 : 1;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (!Globals.MapScene.SelectedCommanders.Contains(this)) return;

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
    
    private class CommanderInventory : Inventory
    {
        private readonly Commander _commander;
        public CommanderInventory(Commander commander) : base()
        {
            _commander = commander;
        }

        public override int GetMaxTransferAmount(string itemType)
        {
            if (!Database.Instance.Units.Any(unit => unit.Name == itemType)) return 0;
            return Mathf.Max(0, _commander.CommandCap - CountItems());
        }
    }
    #endregion
    
    #region Save/Load
    /******************************************************************
     * Save/Load                                                      *
     ******************************************************************/
    
    public virtual Godot.Collections.Dictionary<string, Variant> Save()
    {
        var dict =  new Godot.Collections.Dictionary<string, Variant>()
        {
            { "ItemType", "Commander" },
            { "Name", CommanderName },
            { "Team", Team },
            { "Units", Units.Save() },
            { "PosX", MapPosition.X },
            { "PosY", MapPosition.Y },
            { "SpawnX", SpawnLocation.X },
            { "SpawnY", SpawnLocation.Y },
            { "TargetX", _targetLocation.X },
            { "TargetY", _targetLocation.Y },
            { "BarracksId", BarracksId }
        };
        
        return dict;
    }
	
    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        var name = nodeData["Name"].ToString();
        var team = nodeData["Team"].ToString();
        var mapPosition = new Vector2I(nodeData["PosX"].AsInt32(), nodeData["PosY"].AsInt32());
        var province = Globals.MapScene.TileMap.GetProvence(mapPosition);

        var commander = new Commander(province, team, name);
        commander.Units.Load(nodeData["Units"].AsGodotDictionary());
        commander.SpawnLocation = new Vector2I(nodeData["SpawnX"].AsInt32(), nodeData["SpawnY"].AsInt32());;
        commander._targetLocation = new Vector2I(nodeData["TargetX"].AsInt32(), nodeData["TargetY"].AsInt32());;
        commander.BarracksId = nodeData["BarracksId"].ToString();
    }
    #endregion
}