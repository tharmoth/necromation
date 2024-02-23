using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class MapToFactoryButton : Button
{
    public override void _Pressed()
    {
        base._Pressed();
        
        Globals.FactoryScene ??= GD.Load<PackedScene>("res://src/main.tscn").Instantiate<Node2D>();
        if (Globals.FactoryScene.GetParent() != GetTree().Root) GetTree().Root.AddChild(Globals.FactoryScene);
        
        var to = Globals.FactoryScene;
        to.Visible = true;
        to.ProcessMode = ProcessModeEnum.Inherit;
        
        var from = Globals.MapScene;
        from.Visible = false;
        from.ProcessMode = ProcessModeEnum.Disabled;

        Globals.FactoryCamera.Enabled = true;
        GUI.Instance.Visible = true;
        
        MapGui.Instance.Visible = false;
        Globals.MapCamera.Enabled = false;
    }
}
