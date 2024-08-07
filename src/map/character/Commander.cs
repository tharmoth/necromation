using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.map.character;

public partial class Commander : Node2D, ITransferTarget
{
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    // Note: If you add more state data here make sure to serialize it in Save/Load
    public readonly string CommanderName;
    public readonly string Team;
    public readonly Inventory Units;
    private Vector2I _targetLocation;
    public Vector2I SpawnLocation = new(50, 50);
    /// <summary> Associates this commander with a barracks in the factory. Used for save/load. </summary>
    public string BarracksId = null;
    
    public Command CurrentCommand = Command.Attack;
    public enum Command
    {
        Attack, HoldAndAttack, Fire, HoldAndFire, FireAndKeepDistance
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
    
    /**************************************************************************
     * RPG Constants     													  *
     **************************************************************************/
    public int CommandCap = 200;
    
    public Commander(string team)
    {
        Units = new CommanderInventory(this);
        Team = team;
        CommanderName = MapUtils.GetRandomCommanderName();
    }
    
    // Deconstructor
    public void Kill()
    {
        QueueFree();
    }
    
    public override void _Ready()
    {
        base._Ready();
        Globals.MapScene.CallDeferred("add_child", _line);
        
        
        // if (Team != "Player") _spriteHolder.Visible = false;
    }
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => Units.CanAcceptItems(item, count);
    public bool CanAcceptItemsInserter(string item, int count = 1) => Units.CanAcceptItemsInserter(item, count);
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
            if (!Database.Instance.IsUnit(itemType)) return 0;
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

        var commander = new Commander(team);
        commander.Units.Load(nodeData["Units"].AsGodotDictionary());
        commander.SpawnLocation = new Vector2I(nodeData["SpawnX"].AsInt32(), nodeData["SpawnY"].AsInt32());;
        commander._targetLocation = new Vector2I(nodeData["TargetX"].AsInt32(), nodeData["TargetY"].AsInt32());;
        commander.BarracksId = nodeData["BarracksId"].ToString();
    }
    #endregion
}