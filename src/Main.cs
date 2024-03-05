using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class Main : Scene
{
	public BuildingTileMap TileMap => GetNode<BuildingTileMap>("%TileMap");
	
	private bool _initialized = false;
	public Timer AttackTimer;
	private bool attacked = false;
	public Province AttackProvince;
	
	public override void _EnterTree()
	{
		base._EnterTree();

		MusicManager.PlayAmbiance();
		
		Globals.Tree.Root.AddChild(new CheatCodes());
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_initialized) return;
		_initialized = true;
		var position = Map.FactoryPosition;
		Globals.Player.GlobalPosition =
			position * BuildingTileMap.TileSize * BuildingTileMap.ProvinceSize
			+ position * BuildingTileMap.TileSize * BuildingTileMap.ProvinceGap
			+ Vector2I.One * BuildingTileMap.TileSize * BuildingTileMap.ProvinceSize / 2;
		Globals.TileMap.AddProvence(position);
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
		// 	AttackProvince = Globals.MapScene.TileMap.GetProvinces().Where(province => province.Owner == "Player")
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
		// 		MapGui.Instance.Battle();
		// 		AttackTimer = null;
		//                  
		// 	};
		// 	AttackTimer.Start();
		// }
	}

	public override void OnClose() {}
}
