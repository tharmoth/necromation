using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class BattleGUI : CanvasLayer
{
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_initialized) SetupKillCounts();
        
        if (Input.IsActionJustPressed("return_to_map")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Map);
        if (Input.IsActionJustPressed("return_to_factory")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
        
        var teams = Globals.BattleScene.TileMap.GetEntities(BattleTileMap.Unit)
            .Select(unit => unit as Unit)
            .Where(unit => unit != null)
            .Select(unit => unit.Team)
            .Distinct()
            .ToList();

        if (teams.Count != 1 || _complete) return;
        _complete = true;
        
        Summary.Display(teams.First() == "Player" ? "Victory!" : "Defeat!",
        playerStats, enemyStats);
    }

    private Dictionary<string, UnitStats> playerStats = new();
    private Dictionary<string, UnitStats> enemyStats = new();
	
    private bool _initialized = false;
    private bool _complete = false;
    
    private void SetupKillCounts()
    {
        _initialized = true;
        var enemies = GetTree()
            .GetNodesInGroup("EnemyUnits")
            .OfType<Unit>()
            .ToList();
		
        foreach (var unit in enemies)
        {
            if (!enemyStats.ContainsKey(unit.UnitType))
            {
                enemyStats[unit.UnitType] = new UnitStats(unit.UnitType);
            }
            enemyStats[unit.UnitType].IncrementCount();

            unit.DeathCallbacks.Add((killer) =>
            {
                if (killer.Team == "Enemy") enemyStats[killer.UnitType].IncrementKills();
                else playerStats[killer.UnitType].IncrementKills();
                enemyStats[unit.UnitType].IncrementDeaths();
            });
        }
		
        var minions = GetTree()
            .GetNodesInGroup("PlayerUnits")
            .OfType<Unit>()
            .ToList();
        
        foreach (var unit in minions)
        {
            if (!playerStats.ContainsKey(unit.UnitType))
            {
                playerStats[unit.UnitType] = new UnitStats(unit.UnitType);
            }
            playerStats[unit.UnitType].IncrementCount();

            unit.DeathCallbacks.Add((killer) =>
            {
                if (killer.Team == "Player") playerStats[killer.UnitType].IncrementKills();
                else enemyStats[killer.UnitType].IncrementKills();
                playerStats[unit.UnitType].IncrementDeaths();
            });
        }
    }
}
