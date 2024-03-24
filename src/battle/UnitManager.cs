using System;
using System.Collections.Generic;
using Godot;

namespace Necromation.battle;

public partial class UnitManager : Node
{
    
    private List<Unit> _units = new();
    private Dictionary<string, List<Unit>> _groups = new() {{"Enemy", new List<Unit>()}, {"Player", new List<Unit>()}};

    public override void _EnterTree()
    {
        base._EnterTree();
        Globals.UnitManager = this;
    }
    
    public void RemoveUnit(Unit unit)
    {
        _units.Remove(unit);
        _groups[unit.Team].Remove(unit);
    }
    
    public void AddUnit(Unit unit)
    {
        var first = true;
        unit.SpriteHolder.TreeEntered += () =>
        {
            if (!first) return;
            first = false;
            unit._Ready();
            _units.Add(unit);
        };
        unit.SpriteHolder.TreeExited += unit._ExitTree;
        Globals.BattleScene.AddChild(unit.SpriteHolder);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
       
        foreach (var csharpNode in _units.ToArray())
        {
            try 
            {
                csharpNode._Process(delta);
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
            }
            
        }
    }
    
    public void AddToGroup(Unit unit, string group)
    {
        _groups[group].Add(unit);
    }
    
    public List<Unit> GetGroup(string group)
    {
        return _groups[group];
    }
    
    public bool IsUnitAlive(Unit unit)
    {
        return _units.Contains(unit);
    }
}