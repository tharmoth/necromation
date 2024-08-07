using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map.character;

public partial class MapSquad : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/map/gui/MapSquad/MapSquad.tscn");
	
	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private Container UnitsList => GetNode<Container>("%UnitsList");
	private Label Title => GetNode<Label>("%Title");
	private Label ActionLabel => GetNode<Label>("%ActionLabel");
	private ColorRect Background => GetNode<ColorRect>("%Background");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Commander _commander;
	private static MapSquad _lastClicked;
	
	/**************************************************************************
	 * Constants          													  *
	 **************************************************************************/
	private readonly Color _selectedColor = new Color(0.5625f, 0.5625f, 0.5625f);
	
	// Static Accessor
	public static void UpdateCommanderList(List<Commander> commanders, Container list)
	{
		list.GetChildren().ToList().ForEach(child => child.QueueFree());
		commanders
			.OrderBy(commander => commander.CommanderName)
			.ToList().ForEach(item => AddCommander(item, list));
	}
	
	// Static Accessor
	private static void AddCommander(Commander commander, Container list)
	{
		var inventoryItem = Scene.Instantiate<MapSquad>();
		inventoryItem.Init(commander);
		list.AddChild(inventoryItem);
	}

	private void Init(Commander commander)
	{
		_commander = commander;
		Title.Text = commander.CommanderName;
		ActionLabel.Text = "Defend";
		Background.Color = Colors.Black;
		
		_commander.Units.Listeners.Add(UpdateUnitList);
		UpdateUnitList();
	}
	
	private void UpdateUnitList()
	{
		MapSquadItemBox.UpdateSquad(_commander.Units, UnitsList);
		if (UnitsList.GetChildren().Count == 0) UnitsList.AddChild(new Label { Text = "No Units!" });
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		// Polling is bad. I'm sorry. I'm just trying to get this done.
		// if (!Globals.MapScene.SelectedCommanders.Contains(_commander) && Background.Color != Colors.Black) Background.Color = Colors.Black;
		// else if (Globals.MapScene.SelectedCommanders.Contains(_commander) && Background.Color != _selectedColor) Background.Color = _selectedColor;
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) return;
		
		
		if (Input.IsKeyPressed(Key.Shift))
		{
			var unit = _lastClicked;
			if (unit == null || unit.GetParent() != GetParent()) return;
			var parentUnits = GetParent().GetChildren().OfType<MapSquad>().ToList();
			var startIndex = parentUnits.ToList().IndexOf(unit);
			var endIndex = parentUnits.ToList().IndexOf(this);
			var range = parentUnits.ToList().GetRange(Math.Min(startIndex, endIndex), Math.Abs(startIndex - endIndex) + 1);
			// range.Select(squad => squad._commander).ToList().ForEach(commander => Globals.MapScene.SelectedCommanders.Add(commander));
		}
		else if (Input.IsKeyPressed(Key.Ctrl))
		{
			// Globals.MapScene.SelectedCommanders.Add(_commander);
		}
		else
		{
			// Globals.MapScene.SelectedCommanders.Clear();
			// Globals.MapScene.SelectedCommanders.Add(_commander);
		}
		_lastClicked = this;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_commander?.Units.Listeners.Remove(UpdateUnitList);
	}
}
