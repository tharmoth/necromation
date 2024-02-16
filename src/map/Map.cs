using Godot;
using System;
using System.Linq;
using Necromation.map;

public partial class Map : Node2D
{
	public override void _Ready()
	{
		MapGlobals.SelectedSprite = GetNode<Sprite2D>("%SelectedSprite");
		MapGlobals.UpdateListeners.Add(UpdateLabel);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		// Strange bug where the gui doesn't catch things?
		if (MapGui.Instance.GuiOpen) return;
		
		if (Input.IsActionJustPressed("left_click"))
		{
			var targetLocation = MapGlobals.TileMap.GlobalToMap(GetGlobalMousePosition());
			
			var provence = MapGlobals.TileMap.GetProvence(targetLocation);
			if (provence == null) return;

			SelectProvince(provence);
		}
	}
	
	private void SelectProvince(Province provence)
	{
		MapGlobals.SelectedProvince = provence;
		MapGlobals.SelectedSprite.GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(provence));
		MapGlobals.UpdateListeners.ForEach(listener => listener());
	}

	private void UpdateLabel()
	{
		var provence = MapGlobals.SelectedProvince;

		var unit = provence.Units.CountAllItems() > 0 ? 
			provence.Units.Items
			.Select(unit => unit.Key + " x" + unit.Value)
			.Aggregate((current, next) => current + "\n" + next) 
			: "no units\n";

		MapGui.Instance.SelectedLabel.Text = provence.Name + "\n" + unit;
	}
}
