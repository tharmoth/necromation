using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class MapToBattleButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();
		ChangeScene();
	}
	
	public static void ChangeScene()
	{
		Globals.BattleScene = GD.Load<PackedScene>("res://src/battle.tscn").Instantiate<Battle>();
		if (Globals.BattleScene.GetParent() != Globals.MapScene.GetTree().Root) Globals.MapScene.GetTree().Root.AddChild(Globals.BattleScene);
		
		var show = Globals.BattleScene;
		show.Visible = true;
		show.ProcessMode = ProcessModeEnum.Inherit;
        
		var hide = Globals.MapScene;
		hide.Visible = false;
		hide.ProcessMode = ProcessModeEnum.Disabled;

		Globals.BattleCamera.Enabled = true;
		Globals.BattleScene.GUI.Visible = true;
		
		Globals.BattleScene.Provence = MapGlobals.SelectedProvince;
        
		MapGui.Instance.Visible = false;
		Globals.MapCamera.Enabled = false;
	}
}
