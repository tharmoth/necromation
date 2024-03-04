using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class Map : Node2D
{
	public override void _EnterTree()
	{
		Globals.MapScene = this;
	}
	
	public override void _Ready()
	{
		Globals.MapCamera = GetNode<Camera2D>("Camera2D");
		MapGlobals.SelectedSprite = GetNode<Sprite2D>("%SelectedSprite");
		
		var provence = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition);
		SelectProvince(provence);
		
		VisibilityChanged += () => SelectProvince(MapGlobals.SelectedProvince);
		// VisibilityChanged += () =>
		// {
		// 	if (Visible) MusicManager.PlayExploration();
		// };
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
}
