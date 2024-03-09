using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class Summary : Control
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/battle/gui/Summary/summary.tscn");

	/*************************************************************************
	 * Child Accessors 													     *
	 *************************************************************************/
	private Label Title => GetNode<Label>("%Title");
	private Container PlayerUnitList => GetNode<Container>("%PlayerUnitList");
	private Container EnemyUnitList => GetNode<Container>("%EnemyUnitList");
	
	// Static Accessor
	public static void Display(string title, Dictionary<string, UnitStats> playerStats, Dictionary<string, UnitStats> enemyStats)
	{
		var gui = Scene.Instantiate<Summary>();
		gui.Init(title, playerStats, enemyStats);
		Globals.BattleScene.Gui.AddChild(gui);
	}

	// Constructor workaround.
	private void Init(string title, Dictionary<string, UnitStats> playerStats, Dictionary<string, UnitStats> enemyStats)
	{
		Title.Text = title;
		UnitRow.AddRows(playerStats.Values, PlayerUnitList);
		UnitRow.AddRows(enemyStats.Values, EnemyUnitList);
	}
}