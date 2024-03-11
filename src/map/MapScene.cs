using System;
using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class MapScene : Scene
{
	/**************************************************************************
	 * Utility Accessors    											      *
	 **************************************************************************/
	public List<Province> Provinces => TileMap.Provinces;
	public List<Commander> Commanders => TileMap.Provinces.SelectMany(province => province.Commanders).ToList();
	public Province FactoryProvince => TileMap.GetProvence(FactoryPosition);
	public static Vector2I FactoryPosition => new(4, 2);
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	public override MapGui Gui => GetNode<MapGui>("%GUI");
	public MapTileMap TileMap => GetNode<MapTileMap>("%TileMap");
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	public readonly List<Action> TurnListeners = new();
	public readonly List<Action> UpdateListeners = new();
	public Sprite2D SelectedSprite;
	public Province SelectedProvince;
	public readonly List<Commander> SelectedCommanders = new();

	public MapScene()
	{
		TurnListeners.Add(() => SelectedCommanders.Clear());
	}

	public override void OnOpen()
	{

	}

	public override void OnClose() {}

	public override void _Ready()
	{
		Globals.MapScene.SelectedSprite = GetNode<Sprite2D>("%SelectedSprite");
		
		var provence = Globals.MapScene.TileMap.GetProvence(MapScene.FactoryPosition);
		SelectProvince(provence);
		
		VisibilityChanged += () => SelectProvince(Globals.MapScene.SelectedProvince);
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		
		// Strange bug where the gui doesn't catch things?
		if (Globals.MapScene.Gui.GuiOpen) return;
		
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
