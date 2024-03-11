using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map.character;

public partial class CommandsGui : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/CommandsGui/CommandsGui.tscn");

	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Label CurrentOrderLabel => GetNode<Label>("%CurrentOrderLabel");
	private Container OrderList => GetNode<Container>("%Orders");
	private Container TargetList => GetNode<Container>("%Targets");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Commander _commander;
	
	// Static Accessor
	public static void Display(Commander commander)
	{
		var gui = Scene.Instantiate<CommandsGui>();
		gui.Init(commander);
		Globals.FactoryScene.Gui.Open(gui);
	}
	
	// Constructor workaround.
	private void Init(Commander commander)
	{
		_commander = commander;

		TargetList.GetChildren().ToList().ForEach(child => child.QueueFree());
		foreach (var targetTypeName in Enum.GetNames(typeof(Commander.Target)))
		{
			Enum.TryParse(targetTypeName, true, out Commander.Target targetType);
			
			var panel = new PanelContainer();
			
			var button = new Button();
			button.Text = targetTypeName;
			button.Pressed += () =>
			{
				_commander.TargetType = targetType;
				UpdateLabel();
			};
			panel.AddChild(button);

			var outline = GD.Load<Control>("res://src/shared/gui/outline.tscn");
			panel.AddChild(outline);
			
			TargetList.AddChild(panel);
		}
		
		OrderList.GetChildren().ToList().ForEach(child => child.QueueFree());
		foreach (var commandName in Enum.GetNames(typeof(Commander.Command)))
		{
			Enum.TryParse(commandName, true, out Commander.Command command);
			
			var panel = new PanelContainer();
			
			var button = new Button();
			button.Text = commandName;
			button.Pressed += () =>
			{
				_commander.CurrentCommand = command;
				UpdateLabel();
			};
			panel.AddChild(button);

			var outline = GD.Load<Control>("res://src/shared/gui/outline.tscn");
			panel.AddChild(outline);
			
			TargetList.AddChild(panel);
		}

		UpdateLabel();
	}

	private void UpdateLabel()
	{
		CurrentOrderLabel.Text = _commander.CurrentCommand.ToString() + " " + _commander.TargetType.ToString();
	}
}
