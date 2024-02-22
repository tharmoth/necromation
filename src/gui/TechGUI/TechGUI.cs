using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.character;

public partial class TechGUI : PanelContainer
{
	[Export] private PackedScene _techPanelScene;
	private VBoxContainer TechList => GetNode<VBoxContainer>("%TechList");
	
	public void Display()
	{
		Visible = true;

		TechList.GetChildren().ToList().ForEach(node => node.Free());
		Globals.Database.Technologies.ToList().ForEach(tech =>
		{
			var panel = _techPanelScene.Instantiate<TechPanel>();
			panel.Tech = tech;
			TechList.AddChild(panel);
		});
	}
}
