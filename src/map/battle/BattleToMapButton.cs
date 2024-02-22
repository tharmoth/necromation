using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class BattleToMapButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();

		Globals.MapScene ??= GD.Load<PackedScene>("res://src/map.tscn").Instantiate();
		if (Globals.MapScene.GetParent() != GetTree().Root) GetTree().Root.AddChild(Globals.MapScene);
		
		Node2D show = (Node2D)Globals.MapScene;
		show.Visible = true;
		show.ProcessMode = ProcessModeEnum.Inherit;
        
		Node2D hide = (Node2D)Globals.BattleScene;
		hide.Visible = false;
		hide.ProcessMode = ProcessModeEnum.Disabled;
		
		Globals.BattleCamera.Enabled = false;
		BattleGlobals.GUI.Visible = false;
		
		MapGui.Instance.Visible = true;
		Globals.MapCamera.Enabled = true;
	}
}
