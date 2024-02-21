using Godot;
using System;

public partial class BattleGUI : CanvasLayer
{
    public override void _EnterTree()
    {
        base._EnterTree();
        BattleGlobals.GUI = this;
    }
}
