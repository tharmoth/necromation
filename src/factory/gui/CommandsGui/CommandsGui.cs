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
	private static readonly PackedScene OutlineScene = GD.Load<PackedScene>("res://src/shared/gui/outline.tscn");
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Label CurrentOrderLabel => GetNode<Label>("%CurrentOrderLabel");
	private Container OrderList => GetNode<Container>("%OrderList");
	private Container TargetList => GetNode<Container>("%TargetList");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Commander _commander;

	// Constructor workaround.
	public void Init(Commander commander)
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

			var outline = OutlineScene.Instantiate<Control>();
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

			var outline = OutlineScene.Instantiate<Control>();
			panel.AddChild(outline);
			
			OrderList.AddChild(panel);
		}

		UpdateLabel();
	}

	private void UpdateLabel()
	{
		CurrentOrderLabel.Text = _commander.CurrentCommand.ToString() + " " + _commander.TargetType.ToString();
	}
}
