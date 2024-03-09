using Godot;
using System;
using System.Linq;
using Necromation;

public partial class TechGUI : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/TechGUI/tech_gui.tscn");
	private static readonly PackedScene TechPanelScene = GD.Load<PackedScene>("res://src/factory/gui/TechGUI/tech_panel.tscn");
	
	/* ***********************************************************************
	 * Child Accessors 													     *
	 * ***********************************************************************/
	private VBoxContainer TechList => GetNode<VBoxContainer>("%TechList");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	// None :D
	
	// Static Accessor
	public static void Display()
	{
		var gui = Scene.Instantiate<TechGUI>();
		gui.Init();
		Globals.FactoryScene.Gui.OpenGui(gui);
	}

	// Constructor workaround.
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
