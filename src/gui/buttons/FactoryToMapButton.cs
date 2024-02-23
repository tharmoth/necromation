using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class FactoryToMapButton : Button
{
	[Export] private PackedScene _scene;
	public override void _Pressed()
	{
		base._Pressed();
		ChangeScene();
	}

	public void ChangeScene()
	{
		var tree = GetTree();
		
		Globals.MapScene ??= _scene.Instantiate<Node2D>();
		if (Globals.MapScene.GetParent() != tree.Root) tree.Root.AddChild(Globals.MapScene);
		Globals.FactoryScene ??= (Node2D)GetTree().CurrentScene;
		
		var fuck = Globals.MapScene;
		fuck.Visible = true;
		fuck.ProcessMode = ProcessModeEnum.Inherit;
        
		var shit = Globals.FactoryScene;
		shit.Visible = false;
		shit.ProcessMode = ProcessModeEnum.Disabled;

		Globals.FactoryCamera.Enabled = false;
		GUI.Instance.Visible = false;
        
		MapGui.Instance.Visible = true;
		Globals.MapCamera.Enabled = true;
		
		MapGlobals.TileMap.MoveUnitsToFromFactory();
	}
	
}
