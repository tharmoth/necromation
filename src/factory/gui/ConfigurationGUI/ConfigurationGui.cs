using Godot;
using System;
using System.Linq;
using Necromation;

public partial class ConfigurationGui : PanelContainer
{
	// The container that is used to display our checkboxes. It starts with dummy items so It should be cleared.
	private Container Options => GetNode<Container>("%Options");
	private HSlider HSlider => GetNode<HSlider>("%HSlider");
	private Label TimeLabel => GetNode<Label>("%TimeLabel");
	
	public static void Display()
	{
		var gui = GD.Load<PackedScene>("res://src/factory/gui/ConfigurationGUI/configuration_gui.tscn").Instantiate<ConfigurationGui>();
		Globals.FactoryScene.Gui.Open(gui);
	}

	public override void _Ready()
	{
		base._Ready();
		
		Options.GetChildren().OfType<CheckBox>().ToList().ForEach(node => node.QueueFree());
		
		var checkBox = new CheckBox();
		checkBox.Text =	"Animate Inserters";
		checkBox.SetPressedNoSignal(Config.AnimateInserters);
		checkBox.Toggled += (toggled) => Config.AnimateInserters = toggled;
		Options.AddChild(checkBox);
		
		var checkBox2 = new CheckBox();
		checkBox2.Text = "Animate Belts";
		checkBox2.SetPressedNoSignal(Config.AnimateBelts);
		checkBox2.Toggled += (toggled) => Config.AnimateBelts = toggled;
		Options.AddChild(checkBox2);
		
		var checkBox3 = new CheckBox();
		checkBox3.Text = "Animate Mines";
		checkBox3.SetPressedNoSignal(Config.AnimateMines);
		checkBox3.Toggled += (toggled) => Config.AnimateMines = toggled;
		Options.AddChild(checkBox3);
		
		var checkBox4 = new CheckBox();
		checkBox4.Text = "Physics Belts";
		checkBox4.SetPressedNoSignal(Config.ProcessBeltsInPhysics);
		checkBox4.Toggled += (toggled) => Config.ProcessBeltsInPhysics = toggled;
		Options.AddChild(checkBox4);
		
		var checkBox5 = new CheckBox();
		checkBox5.Text = "Cinematic Mode";
		checkBox5.SetPressedNoSignal(Config.CinematicMode);
		checkBox5.Toggled += (toggled) =>
		{
			Config.CinematicMode = toggled;
			Globals.FactoryScene.Gui.ToggleCinematicMode();
		};
		Options.AddChild(checkBox5);
		
		HSlider.Value = Globals.FactoryScene.DayNight.GetHour();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		var dayNight = Globals.FactoryScene.DayNight;
		dayNight.SetHour((float)HSlider.Value); 
		TimeLabel.Text = $"{dayNight.GetHour()}:00";
	}
}
