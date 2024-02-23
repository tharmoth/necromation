using Godot;
using System;
using System.Linq;
using Necromation;

public partial class BattleGUI : CanvasLayer
{
    private Label BattleCompleteLabel => GetNode<Label>("%BattleCompleteLabel");
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Globals.BattleScene.GUI = this;
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);

        var teams = Globals.BattleScene.TileMap.GetEntities(BattleTileMap.Unit)
            .Select(unit => unit as Unit)
            .Where(unit => unit != null)
            .Select(unit => unit.Team)
            .Distinct()
            .ToList();

        if (teams.Count != 1 || BattleCompleteLabel.Visible) return;
        BattleCompleteLabel.Text = teams.First() == "Player" ? "Victory!" : "Defeat!";
        BattleCompleteLabel.Visible = true;
    }
}
