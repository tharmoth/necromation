using Godot;
using System;
using Necromation;

public partial class TempBattlePlayerController : Node2D
{
	public override void _Ready()
	{
		base._Ready();
		Globals.BattleCamera = GetParent().GetNode<Camera2D>("Camera2D");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (!Input.IsActionJustPressed("left_click")) return;

		BattleGlobals.TempUnit.TargetPosition = BattleGlobals.TileMap.GlobalToMap(GetGlobalMousePosition());
	}
}
