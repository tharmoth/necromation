using Godot;
using System;
using Necromation;

public partial class CloseButton : Button
{
	public override void _Pressed()
	{
		base._Pressed(); 
		Globals.FactoryScene.Gui.CloseGui();
	}
}
