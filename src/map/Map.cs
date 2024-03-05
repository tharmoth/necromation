using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class Map : Scene
{
	public MapTileMap TileMap => GetNode<MapTileMap>("%TileMap");

	public override void OnOpen()
	{
		if (Globals.TileMap == null) return;
		
		var prov = TileMap.GetProvence(MapGlobals.FactoryPosition);
		foreach (var barracks in Globals.TileMap.GetEntitiesOfType(nameof(Barracks)).OfType<Barracks>())
		{
			var inventory = barracks.GetInventories().First();
			Inventory.TransferAllTo(inventory, prov.Units);
		}
	}

	public override void OnClose() {}

	public override void _Ready()
	{
		MapGlobals.SelectedSprite = GetNode<Sprite2D>("%SelectedSprite");
		
		var provence = Globals.MapScene.TileMap.GetProvence(MapGlobals.FactoryPosition);
		SelectProvince(provence);
		
		VisibilityChanged += () => SelectProvince(MapGlobals.SelectedProvince);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		// Strange bug where the gui doesn't catch things?
		if (MapGui.Instance.GuiOpen) return;
		
		if (Input.IsActionJustPressed("left_click"))
		{
			var targetLocation = Globals.MapScene.TileMap.GlobalToMap(GetGlobalMousePosition());
			
			var provence = Globals.MapScene.TileMap.GetProvence(targetLocation);
			if (provence == null) return;

			SelectProvince(provence);
		}
	}

	private void SelectProvince(Province provence)
	{
		MapGlobals.SelectedProvince = provence;
		MapGlobals.SelectedSprite.GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(Globals.MapScene.TileMap.GetLocation(provence));
		MapGlobals.UpdateListeners.ForEach(listener => listener());
	}
}
