using Godot;
using System.Collections.Generic;
using Necromation;

public partial class BattleGUI : CanvasLayer
{
    /**************************************************************************
     * State Data          													  *
     **************************************************************************/
    private readonly Dictionary<string, UnitStats> _playerStats = new();
    private readonly Dictionary<string, UnitStats> _enemyStats = new();
	
    private bool _initialized = false;
    private bool _complete = false;

    private const double MusteringDelay = 3;
    private double _time = 0;
    private bool _startCountdown = false;
    public bool Started = false;
    
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_startCountdown) _time += delta;
        if (_time > MusteringDelay) Started = true;
        
        if (!_initialized) SetupKillCounts();

        var playerUnitCount = Globals.UnitManager.GetGroup("Player").Count;
        var enemyUnitCount = Globals.UnitManager.GetGroup("Enemy").Count;

        if ((playerUnitCount != 0 && enemyUnitCount != 0) || _complete) return;
        _complete = true;
        
        if (playerUnitCount == 0) Summary.Display("Defeat!", _playerStats, _enemyStats);
        else if (enemyUnitCount == 0) Summary.Display("Victory!", _playerStats, _enemyStats);
        else _complete = false;
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("return_to_map")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Map);
        if (inputEvent.IsActionPressed("return_to_factory")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
    }

    private void SetupKillCounts()
    {
        _initialized = true;
        _startCountdown = true;
        var enemies = Globals.UnitManager.GetGroup("Enemy");
		
        foreach (var unit in enemies)
        {
            if (!_enemyStats.ContainsKey(unit.UnitType))
            {
                _enemyStats[unit.UnitType] = new UnitStats(unit.UnitType);
            }
            _enemyStats[unit.UnitType].IncrementCount();

            unit.DeathCallbacks.Add((killer) =>
            {
                if (killer.Team == "Enemy") _enemyStats[killer.UnitType].IncrementKills();
                else _playerStats[killer.UnitType].IncrementKills();
                _enemyStats[unit.UnitType].IncrementDeaths();
                Globals.Souls++;
            });
        }

        var minions = Globals.UnitManager.GetGroup("Player");
        
        foreach (var unit in minions)
        {
            if (!_playerStats.ContainsKey(unit.UnitType))
            {
                _playerStats[unit.UnitType] = new UnitStats(unit.UnitType);
            }
            _playerStats[unit.UnitType].IncrementCount();

            unit.DeathCallbacks.Add((killer) =>
            {
                if (killer.Team == "Player") _playerStats[killer.UnitType].IncrementKills();
                else _enemyStats[killer.UnitType].IncrementKills();
                _playerStats[unit.UnitType].IncrementDeaths();
            });
        }
    }
}
