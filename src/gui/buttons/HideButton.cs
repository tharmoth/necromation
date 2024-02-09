using Godot;
using System;

public partial class HideButton : Button
{
	[Export] private Control nodeToHide;

	public override void _Pressed()
	{
		base._Pressed();
		nodeToHide.Visible = false;
	}
}
