using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class MapToBattleButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();

		Globals.BattleScene ??= GD.Load<PackedScene>("res://src/battle.tscn").Instantiate();
		if (Globals.BattleScene.GetParent() != GetTree().Root) GetTree().Root.AddChild(Globals.BattleScene);
		
		Node2D show = (Node2D)Globals.BattleScene;
		show.Visible = true;
		show.ProcessMode = ProcessModeEnum.Inherit;
        
		Node2D hide = (Node2D)Globals.MapScene;
		hide.Visible = false;
		hide.ProcessMode = ProcessModeEnum.Disabled;

		Globals.BattleCamera.Enabled = true;
		BattleGlobals.GUI.Visible = true;
		
		BattleGlobals.Provence = MapGlobals.SelectedProvince;
        
		MapGui.Instance.Visible = false;
		Globals.MapCamera.Enabled = false;
	}
}
