using System;
using System.Collections.Generic;
using Godot;

namespace Necromation.factory;

public partial class BuildingManager : Node
{
    private List<Building> _buildings = new();

    public override void _EnterTree()
    {
        base._EnterTree();
        Globals.BuildingManager = this;
    }
    
    public void RemoveBuilding(Building building)
    {
        _buildings.Remove(building);
    }
    
    public void AddBuilding(Building building, Vector2 globalPosition)
    {
        var first = true;
        building.Sprite.TreeEntered += () =>
        {
            if (!first) return;
            first = false;
            building._Ready();
            _buildings.Add(building);
        };
        building.Sprite.TreeExited += building._ExitTree;
        building.GlobalPosition = globalPosition;
        Globals.FactoryScene.AddChild(building.Sprite);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
       
        foreach (var building in _buildings)
        {
            try 
            {
                building._Process(delta);
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
            }
            
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        foreach (var building in _buildings)
        {
            try 
            {
                building._PhysicsProcess(delta);
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
            }
            
        }
    }

    public virtual Godot.Collections.Dictionary<string, Variant> Save()
    {
        var dict = new Godot.Collections.Dictionary<string, Variant>();
        dict["ItemType"] = "BuildingManager";
        for (var i = 0; i < _buildings.Count; i++)
        {
            dict["Building" + i] = _buildings[i].Save();
        }
        return dict;
    }

    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {

        foreach (var building in Globals.BuildingManager._buildings)
        {
            building.Remove(null);
        }
        
        int index = 0;
        while (nodeData.ContainsKey("Building" + index))
        {
            var buildingData = (Godot.Collections.Dictionary<string, Variant>) nodeData["Building" + index];
            Building.Load(buildingData);
            index++;
        }
    }
}