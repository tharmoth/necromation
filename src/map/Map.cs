using System;
using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class Map : Scene
{
	public MapTileMap TileMap => GetNode<MapTileMap>("%TileMap");
	
	public readonly List<Action> TurnListeners = new();
	public readonly List<Action> UpdateListeners = new();
    
	public Sprite2D SelectedSprite;
	public Province SelectedProvince;
	public Commander SelectedCommander;

	public static Vector2I FactoryPosition => new(4, 2);

	public Map()
	{
		TurnListeners.Add(() => SelectedCommander = null);
	}

	public override void OnOpen()
	{
		if (Globals.TileMap == null) return;
		
		var prov = TileMap.GetProvence(Map.FactoryPosition);
		foreach (var barracks in Globals.TileMap.GetEntitiesOfType(nameof(Barracks)).OfType<Barracks>())
		{
			var inventory = barracks.GetInventories().First();
			Inventory.TransferAllTo(inventory, prov.Units);
		}
	}

	public override void OnClose() {}

	public override void _Ready()
	{
		Globals.MapScene.SelectedSprite = GetNode<Sprite2D>("%SelectedSprite");
		
		var provence = Globals.MapScene.TileMap.GetProvence(Map.FactoryPosition);
		SelectProvince(provence);
		
		VisibilityChanged += () => SelectProvince(Globals.MapScene.SelectedProvince);
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
		Globals.MapScene.SelectedProvince = provence;
		Globals.MapScene.SelectedSprite.GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(Globals.MapScene.TileMap.GetLocation(provence));
		Globals.MapScene.UpdateListeners.ForEach(listener => listener());
	}
}
