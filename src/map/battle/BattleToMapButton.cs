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
		Globals.BattleScene.Provence.Commanders.Where(commander => commander.Team != team).ToList().ForEach(commander => commander.Kill());
		Globals.BattleScene.QueueFree();
		Globals.BattleScene = null;
	}
}
