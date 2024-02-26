using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class MapToFactoryButton : Button
{
    public override void _Ready()
    {
        base._Ready();
        MapGlobals.UpdateListeners.Add(Update);
    }

    public override void _Pressed()
    {
        base._Pressed();

        ChangeScene();
    }

    public static void ChangeScene()
    {
        if (Globals.ChangingScene) return;
        Globals.ChangingScene = true;
        
        Globals.FactoryScene ??= GD.Load<PackedScene>("res://src/main.tscn").Instantiate<Node2D>();
        if (Globals.FactoryScene.GetParent() != Globals.MapScene.GetTree().Root) Globals.MapScene.GetTree().Root.AddChild(Globals.FactoryScene);
        
        var to = Globals.FactoryScene;
        to.Visible = true;
        to.ProcessMode = ProcessModeEnum.Inherit;
        
        var from = Globals.MapScene;
        from.Visible = false;
        from.ProcessMode = ProcessModeEnum.Disabled;

        Globals.FactoryCamera.Enabled = true;
        FactoryGUI.Instance.Visible = true;
        
        MapGui.Instance.Visible = false;
        Globals.MapCamera.Enabled = false;
        
        Globals.ChangingScene = false;
        
        // We need  to wait or this will be called again once the map loads and it sees inputjustpressed.
        Globals.FactoryScene.GetTree().CreateTimer(.1).Timeout += () => Globals.ChangingScene = false;
    }

    private void Update()
    {
        Visible = MapGlobals.TileMap.GetLocation(MapGlobals.SelectedProvince) == MapGlobals.FactoryPosition;
    }
}
