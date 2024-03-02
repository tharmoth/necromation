using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class Summary : Control
{
	private Label Title => GetNode<Label>("%Title");
	private Container UnitList => GetNode<Container>("%UnitList");
	
	public static Summary Display(string title, Dictionary<string, UnitStats> playerStats, Dictionary<string, UnitStats> enemyStats)
	{
		var gui = GD.Load<PackedScene>("res://src/map/battle/gui/Summary/summary.tscn").Instantiate<Summary>();
		gui.Init(title, playerStats, enemyStats);
		Globals.BattleScene.GUI.AddChild(gui);
		return gui;
	}

	private void Init(string title, Dictionary<string, UnitStats> playerStats, Dictionary<string, UnitStats> enemyStats)
	{
		Title.Text = title;
		UnitList.GetChildren().ToList().ForEach(child => child.QueueFree());
		
		var playerLabel = new Label();
		playerLabel.Text = "Player";
		playerLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		playerLabel.HorizontalAlignment = HorizontalAlignment.Center;
		UnitList.AddChild(playerLabel);
		
		foreach (var stat in playerStats)
		{
			var row = GD.Load<PackedScene>("res://src/map/battle/gui/Summary/unitrow.tscn").Instantiate<UnitRow>();
			row.Init(stat.Value);
			UnitList.AddChild(row);
		}
		
		var enemyLabel = new Label();
		enemyLabel.Text = "Enemy";
		enemyLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		enemyLabel.HorizontalAlignment = HorizontalAlignment.Center;
		UnitList.AddChild(enemyLabel);

		foreach (var stat in enemyStats)
		{
			var row = GD.Load<PackedScene>("res://src/map/battle/gui/Summary/unitrow.tscn").Instantiate<UnitRow>();
			row.Init(stat.Value);
			UnitList.AddChild(row);
		}
	}
	
	public class UnitStats
	{
		public string UnitType { get; set; }
		public int Count { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }

		public UnitStats(string unitType)
		{
			UnitType = unitType;
			Count = 0;
			Kills = 0;
			Deaths = 0;
		}

		public void IncrementCount() => Count++;
		public void IncrementKills() => Kills++;
		public void IncrementDeaths() => Deaths++;
	}
}
