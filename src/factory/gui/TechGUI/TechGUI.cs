using Godot;
using System;
using System.Linq;
using Necromation;

public partial class TechGUI : PanelContainer
{
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/TechGUI/tech_gui.tscn");
	private static readonly PackedScene TechPanelScene = GD.Load<PackedScene>("res://src/factory/gui/TechGUI/tech_panel.tscn");
	
	private VBoxContainer TechList => GetNode<VBoxContainer>("%TechList");

	
	public static void Display()
	{
		var gui = Scene.Instantiate<TechGUI>();
		gui.Init();
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	private void Init()
	{
		// Clear out the placeholder data.
		TechList.GetChildren().ToList().ForEach(node => node.Free());
		
		Database.Instance.Technologies.ToList().ForEach(tech =>
		{
			var panel = TechPanelScene.Instantiate<TechPanel>();
			panel.Tech = tech;
			TechList.AddChild(panel);
		});
	}
}
