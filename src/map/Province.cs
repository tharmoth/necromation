﻿using System.Collections.Generic;
using Godot;
using Necromation.map.character;

namespace Necromation.map;

public partial class Province : Node, ITransferTarget
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

    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1, Vector2 position = default) => Units.CanAcceptItems(item, count, position);
    public void Insert(string item, int count = 1, Vector2 position = default) => Units.Insert(item, count, position);
    public bool Remove(string item, int count = 1) => Units.Remove(item, count);
    public string GetFirstItem() => Units.GetFirstItem();
    public List<Inventory> GetInventories() => Units.GetInventories();
    #endregion
}