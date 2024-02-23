using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class MapToBattleButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();

		Globals.BattleScene ??= GD.Load<PackedScene>("res://src/battle.tscn").Instantiate<Battle>();
		if (Globals.BattleScene.GetParent() != GetTree().Root) GetTree().Root.AddChild(Globals.BattleScene);
		
		Node2D show = Globals.BattleScene;
		show.Visible = true;
		show.ProcessMode = ProcessModeEnum.Inherit;
        
		Node2D hide = (Node2D)Globals.MapScene;
		hide.Visible = false;
		hide.ProcessMode = ProcessModeEnum.Disabled;

		Globals.BattleCamera.Enabled = true;
		Globals.BattleScene.GUI.Visible = true;
		
		Globals.BattleScene.Provence = MapGlobals.SelectedProvince;
        
		MapGui.Instance.Visible = false;
		Globals.MapCamera.Enabled = false;
	}
}
