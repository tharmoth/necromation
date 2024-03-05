using Godot;
using System;
using System.Linq;
using Necromation;

public partial class TechGUI : PanelContainer
{
	[Export] private PackedScene _techPanelScene;
	private VBoxContainer TechList => GetNode<VBoxContainer>("%TechList");
	
	public void Display()
	{
		Visible = true;

		TechList.GetChildren().ToList().ForEach(node => node.Free());
		Database.Instance.Technologies.ToList().ForEach(tech =>
		{
			var panel = _techPanelScene.Instantiate<TechPanel>();
			panel.Tech = tech;
			TechList.AddChild(panel);
		});
	}
}
