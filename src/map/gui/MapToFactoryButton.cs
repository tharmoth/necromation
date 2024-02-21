using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class MapToFactoryButton : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        
        Globals.FactoryScene ??= GD.Load<PackedScene>("res://src/main.tscn").Instantiate();
        if (Globals.FactoryScene.GetParent() != GetTree().Root) GetTree().Root.AddChild(Globals.FactoryScene);
        
        Node2D to = (Node2D)Globals.FactoryScene;
        to.Visible = true;
        to.ProcessMode = ProcessModeEnum.Inherit;
        
        Node2D from = (Node2D)Globals.MapScene;
        from.Visible = false;
        from.ProcessMode = ProcessModeEnum.Disabled;

        Globals.Camera.Enabled = true;
        GUI.Instance.Visible = true;
        
        MapGui.Instance.Visible = false;
        MapGlobals.Camera.Enabled = false;
    }
}
