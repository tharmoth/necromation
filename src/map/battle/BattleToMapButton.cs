using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class BattleToMapButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();

		ChangeScene();
	}
	
	public static void ChangeScene()
	{
		Globals.MapScene ??= GD.Load<PackedScene>("res://src/map.tscn").Instantiate<Node2D>();
		if (Globals.MapScene.GetParent() != Globals.BattleScene.GetTree().Root) Globals.BattleScene.GetTree().Root.AddChild(Globals.MapScene);
		
		var show = Globals.MapScene;
		show.Visible = true;
		show.ProcessMode = ProcessModeEnum.Inherit;
        
		var hide = Globals.BattleScene;
		hide.Visible = false;
		hide.ProcessMode = ProcessModeEnum.Disabled;
		
		Globals.BattleCamera.Enabled = false;
		Globals.BattleScene.GUI.Visible = false;
		
		MapGui.Instance.Visible = true;
		Globals.MapCamera.Enabled = true;

		var team = Globals.BattleScene.TileMap.GetEntities(BattleTileMap.Unit)
			.Select(unit => unit as Unit)
			.Where(unit => unit != null)
			.Select(unit => unit.Team)
			.Distinct()
			.First();

		Globals.BattleScene.Provence.Owner = team;
		var deadCommanders =
			Globals.BattleScene.Provence.Commanders.Where(commander => commander.Team != team).ToList();
		deadCommanders.ForEach(commander => Globals.BattleScene.Provence.Commanders.Remove(commander));
		deadCommanders.ForEach(commander => commander.QueueFree());
		Globals.BattleScene.QueueFree();
		Globals.BattleScene = null;

		if (team == "Player" && MapGlobals.TileMap.GetProvinces().Count(province => province.Owner == "Player") == 2)
		{
			var provenceUp = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 2);
			var provenceDown = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 2);
			var provenceLeft = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 2);
			var provenceRight = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 2);
		}
		
		if (team == "Player" && MapGlobals.TileMap.GetProvinces().Count(province => province.Owner == "Player") == 3)
		{
			var provenceUp = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Archer", 5);
			var provenceDown = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Archer", 5);
			var provenceLeft = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Archer", 5);
			var provenceRight = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Archer", 5);
			
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 5);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 5);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 5);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 5);
		}
		
		if (team == "Player" && MapGlobals.TileMap.GetProvinces().Count(province => province.Owner == "Player") == 4)
		{
			var provenceUp = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Up);
			if (provenceUp.Owner != "Player") provenceUp.Commanders.First().Insert("Infantry", 5);
			var provenceDown = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Down);
			if (provenceDown.Owner != "Player") provenceDown.Commanders.First().Insert("Infantry", 5);
			var provenceLeft = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Left);
			if (provenceLeft.Owner != "Player") provenceLeft.Commanders.First().Insert("Infantry", 5);
			var provenceRight = MapGlobals.TileMap.GetProvence(MapGlobals.FactoryPosition + Vector2I.Right);
			if (provenceRight.Owner != "Player") provenceRight.Commanders.First().Insert("Infantry", 5);
		}
		
		MapGlobals.UpdateListeners.ForEach(listener => listener());
	}
}
