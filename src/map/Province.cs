using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.map.character;
using Necromation.map.gui;

namespace Necromation.map;

public partial class Province : Node, Inserter.ITransferTarget
{
    public string Name { get; } = MapUtils.GetRandomProvinceName();
    public readonly Dictionary<string, int> RecruitQueue = new();
    public readonly Inventory Units = new();
    public readonly List<Commander> Commanders = new();
    
    public Province()
    {
        MapGlobals.TurnListeners.Add(OnTurnEnd);
    }

    public void Recruit(string type)
    {
        RecruitQueue.TryGetValue(type, out var currentCount);
        RecruitQueue[type] = currentCount + 1;
    }
    
    public void OnTurnEnd()
    {
        foreach (var (type, count) in RecruitQueue)
        {
            if (type == "commander")
            {
                for (var i = 0; i < count; i++)
                {
                    var commander = new Commander(this);
                    Commanders.Add(commander);
                    GetTree().Root.AddChild(commander);
                }
            }
            else
            {
                Units.Insert(type, count);
            }
        }
        RecruitQueue.Clear();
    }


    public Inventory GetInputInventory()
    {
        return Units;
    }

    public Inventory GetOutputInventory()
    {
        return Units;
    }

    public bool CanAcceptItem(string item)
    {
        return true;
    }
}