using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class BattleToMapButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();

		Globals.MapScene ??= GD.Load<PackedScene>("res://src/map.tscn").Instantiate<Node2D>();
		if (Globals.MapScene.GetParent() != GetTree().Root) GetTree().Root.AddChild(Globals.MapScene);
		
		var show = Globals.MapScene;
		show.Visible = true;
		show.ProcessMode = ProcessModeEnum.Inherit;
        
		var hide = Globals.BattleScene;
		hide.Visible = false;
		hide.ProcessMode = ProcessModeEnum.Disabled;
		
		Globals.BattleCamera.Enabled = false;
		Globals.BattleScene.Visible = false;
		
		MapGui.Instance.Visible = true;
		Globals.MapCamera.Enabled = true;
	}
}
