using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class UnitRow : HBoxContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/battle/gui/Summary/unitrow.tscn");
	
	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private TextureRect Sprite => GetNode<TextureRect>("%Sprite");
	private Label NameLabel => GetNode<Label>("%Name");
	private Label CountLabel => GetNode<Label>("%Count");
	private Label KillsLabel => GetNode<Label>("%Kills");
	private Label DeathsLabel => GetNode<Label>("%Deaths");

	// Static Accessor
	public static void AddRows(IEnumerable<UnitStats> stats, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());
		stats.ToList().ForEach(stat => UnitRow.AddRow(stat, list));
	}
	
	// Static Accessor
	private static void AddRow(UnitStats stat, Container list)
	{
		var row = Scene.Instantiate<UnitRow>();
		row.Init(stat);
		list.AddChild(row);
	}
	
	// Constructor workaround.
	private void Init(UnitStats stat)
	{
		Sprite.Texture = Database.Instance.GetTexture(stat.UnitType);
		NameLabel.Text = stat.UnitType;
		CountLabel.Text = stat.Count.ToString();
		KillsLabel.Text = stat.Kills.ToString();
		DeathsLabel.Text = stat.Deaths.ToString();
	}
}
