using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple item box that displays a unit. It only differs from the ItemBox in that it is not clickable.
/// </summary>
public partial class MapSquadItemBox : ItemBox
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/map/gui/MapSquad/MapSquadItemBox.tscn");
	
	// Static Accessor
	public static void UpdateSquad(Inventory from, Container list)
	{
		// Free instead of queuefree so that MapSquad can detect if empty.
		list.GetChildren().ToList().ForEach(child => child.Free());
		from.Items
			.OrderBy(item => item.Key)
			.ToList().ForEach(item => AddItem(item, list));
	}
	
	// Static Accessor
	private static void AddItem(KeyValuePair<string, int> item, Container list)
	{
		var inventoryItem = Scene.Instantiate<MapSquadItemBox>();
		inventoryItem.Init(item.Key, item.Value);
		list.AddChild(inventoryItem);
	}
	
	public override void _Ready()
	{
		Button.Visible = false;
	}
}
