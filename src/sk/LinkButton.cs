using Godot;
using System;

public partial class LinkButton : Button
{
	[Export] private string _url = "";

	public override void _Pressed()
	{
		base._Pressed();
		OS.ShellOpen(_url);
	}
}
