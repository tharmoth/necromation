using Godot;
using System;

public partial class HideButton : Button
{
	[Export] private Control _nodeToHide;

	public override void _Pressed()
	{
		base._Pressed();
		_nodeToHide.Visible = false;
	}
}
