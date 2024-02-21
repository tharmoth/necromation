using Godot;
using System;
using Necromation;

public partial class Battle : Node2D
{
	public override void _EnterTree()
	{
		Globals.BattleScene = this;
	}
}
