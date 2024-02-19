using Godot;
using System;
using Necromation;

public partial class TempBattlePlayerController : Node2D
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (!Input.IsActionJustPressed("left_click")) return;

		BattleGlobals.TempUnit.TargetPosition = BattleGlobals.TileMap.GlobalToMap(GetGlobalMousePosition());
	}
}
