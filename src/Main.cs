using Godot;
using System;
using Necromation;

public partial class Main : Node2D
{
	public override void _EnterTree()
	{
		Globals.FactoryScene = this;
	}
}
