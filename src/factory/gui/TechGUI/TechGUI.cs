using Godot;
using System;
using System.Linq;
using Necromation;

public partial class TechGUI : PanelContainer
{
	private VBoxContainer TechList => GetNode<VBoxContainer>("%TechList");
	
	public void Display()
	{
		Visible = true;

		TechList.GetChildren().ToList().ForEach(node => node.Free());
		Database.Instance.Technologies.ToList().ForEach(tech =>
		{
			var panel = GD.Load<PackedScene>("res://src/factory/gui/TechGUI/tech_panel.tscn").Instantiate<TechPanel>();
			panel.Tech = tech;
			TechList.AddChild(panel);
		});
	}
}
