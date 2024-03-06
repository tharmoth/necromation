using Godot;
using System;

public partial class DropShadowBorder : PanelContainer
{
	
	private Panel BlurPanel => GetNode<Panel>("%BlurPanel");
	public void DisableBlur()
	{
		BlurPanel.Visible = false;
	}
	
	public void EnableBlur()
	{
		BlurPanel.Visible = true;
	}
}
