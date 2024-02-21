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
		var tree = GetTree();
		
		Globals.MapScene ??= _scene.Instantiate();
		if (Globals.MapScene.GetParent() != tree.Root) tree.Root.AddChild(Globals.MapScene);
		Globals.FactoryScene ??= GetTree().CurrentScene;
		
		Node2D fuck = (Node2D)Globals.MapScene;
		fuck.Visible = true;
		fuck.ProcessMode = ProcessModeEnum.Inherit;
        
		Node2D shit = (Node2D)Globals.FactoryScene;
		shit.Visible = false;
		shit.ProcessMode = ProcessModeEnum.Disabled;

		Globals.Camera.Enabled = false;
		GUI.Instance.Visible = false;
        
		MapGui.Instance.Visible = true;
		MapGlobals.Camera.Enabled = true;
		
		MapGlobals.TileMap.MoveUnitsToFromFactory();
	}
	
}
