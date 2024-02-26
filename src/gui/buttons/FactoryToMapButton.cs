using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class FactoryToMapButton : Button
{
	public override void _Pressed()
	{
		base._Pressed();
		ChangeScene();
	}

	public static void ChangeScene()
	{
		if (Globals.ChangingScene) return;
		Globals.ChangingScene = true;
        
		Globals.MapScene ??= GD.Load<PackedScene>("res://src/map.tscn").Instantiate<Node2D>();
		if (Globals.MapScene.GetParent() != Globals.FactoryScene.GetTree().Root) Globals.FactoryScene.GetTree().Root.AddChild(Globals.MapScene);
		
		var to = Globals.MapScene;
		to.Visible = true;
		to.ProcessMode = ProcessModeEnum.Inherit;
        
		var from = Globals.FactoryScene;
		from.Visible = false;
		from.ProcessMode = ProcessModeEnum.Disabled;

		Globals.FactoryCamera.Enabled = false;
		FactoryGUI.Instance.Visible = false;
        
		MapGui.Instance.Visible = true;
		Globals.MapCamera.Enabled = true;
		
		MapGlobals.TileMap.MoveUnitsToFromFactory();

		// We need  to wait or this will be called again once the map loads and it sees inputjustpressed.
		// I really need to build that scene manager. this is getting out of hand.
		Globals.MapScene.GetTree().CreateTimer(.1).Timeout += () => Globals.ChangingScene = false;
	}
	
}
