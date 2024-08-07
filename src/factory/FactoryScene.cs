using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;
using Necromation.map;
using Necromation.map.character;
using Necromation.sk;

public partial class FactoryScene : Scene
{
	public static bool ShouldLoad;
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	public CraftingQueue CraftingQueue => GetNode<CraftingQueue>("%CraftingQueue");

	private FactoryTileMap _map;
	public FactoryTileMap TileMap
	{
		get => _map ??= GetNode<FactoryTileMap>("%TileMap");
		private set => _map = value;
	}
	


	public Node2D GroundItemHolder;
	public DayNight DayNight => GetNode<DayNight>("%DayNight");
	
	private CanvasLayer CursorLayer => GetNode<CanvasLayer>("%CursorLayer");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private bool _initialized = false;
	public Timer AttackTimer;
	private bool attacked = false;
	public Province AttackProvince;
	

	public override void _EnterTree()
	{
		base._EnterTree();
		MusicManager.PlayAmbiance();
	}

	public override void _Ready()
	{
		base._Ready();
		// Cache this here for performance reasons so we don't have to access the tree.
		GroundItemHolder = GetNode<Node2D>("GroundItemHolder");
		// GetNode<GpuParticles2D>("%Party").SpeedScale = 0;
		
		var power = new PowerSystem();
		CallDeferred("add_child", power);

		var building = new BuildingSystem();
		CallDeferred("add_child", building);
		
		// Used for loading from main menu. Not sure if good idea. Think on this.
		if (ShouldLoad)
		{
			CallDeferred("LoadTest");
			ShouldLoad = false;
		}
	}

	private void LoadTest()
	{
		SKSaveLoad.LoadGame(this);
	}

	public override FactoryGUI Gui => GetNode<FactoryGUI>("%GUI");

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_initialized) return;
		_initialized = true;

		
		
		var provence = Globals.MapScene
			.Map
			.Provinces
			.First(province => province.Owner == "Player");

		var provencePercent = provence.PositionPercent;
		
		var factorySize = TileMap.GetUsedRect().End * TileMap.TileSet.TileSize;
		var factoryPosition = factorySize * provencePercent;

		Globals.Player.GlobalPosition = factoryPosition;
	}

	public override void OnOpen()
	{
		MusicManager.PlayAmbiance();
		TileMap.OnOpen();
		CursorLayer.Visible = true;
	}
	

	public override void OnClose()
	{
		CursorLayer.Visible = false;
	}
}
