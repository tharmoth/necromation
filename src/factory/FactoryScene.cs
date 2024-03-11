using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.gui;
using Necromation.map;
using Necromation.map.character;

public partial class FactoryScene : Scene
{
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	public CraftingQueue CraftingQueue => GetNode<CraftingQueue>("%CraftingQueue");
	public FactoryTileMap TileMap => GetNode<FactoryTileMap>("%TileMap");
	public Node2D GroundItemHolder;
	
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
	}

	public override FactoryGUI Gui => GetNode<FactoryGUI>("%GUI");

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_initialized) return;
		_initialized = true;
		var position = MapScene.FactoryPosition;
		Globals.Player.GlobalPosition =
			position * FactoryTileMap.TileSize * FactoryTileMap.ProvinceSize
			+ position * FactoryTileMap.TileSize * FactoryTileMap.ProvinceGap
			+ Vector2I.One * FactoryTileMap.TileSize * FactoryTileMap.ProvinceSize / 2;
		Globals.FactoryScene.TileMap.AddProvence(position);
	}

	public override void OnOpen()
	{
		MusicManager.PlayAmbiance();
		TileMap.OnOpen();
		
		// if (AttackTimer == null && Globals.MapScene.TileMap
		// 	    .GetProvinces().Count(province => province.Owner == "Player") >= 4)
		// {
		// 	attacked = true;
		// 	AttackTimer = new Timer();
		// 	AddChild(AttackTimer);
		// 	AttackProvince = Globals.MapScene.TileMap.Provinces.Where(province => province.Owner == "Player")
		// 		.MinBy(_ => GD.Randf());
		// 	
		// 	GD.Print("attack incoming!");
		// 	
		// 	AttackTimer.WaitTime = 60*5;
		// 	AttackTimer.Timeout += () =>
		// 	{
		// 		GD.Print("Under attack!");
		// 		AttackTimer.QueueFree();
		//
		// 		var attackingCommander = new Commander(AttackProvince, "Enemy");
		// 		attackingCommander.Units.Insert("Rabble", 10);
		// 		attackingCommander.GlobalPosition = AttackProvince.GlobalPosition;
		// 		AttackProvince.Commanders.Add(attackingCommander);
		// 		Globals.MapScene.Gui.Battle();
		// 		AttackTimer = null;
		//                  
		// 	};
		// 	AttackTimer.Start();
		// }
	}

	public override void OnClose() {}
}
