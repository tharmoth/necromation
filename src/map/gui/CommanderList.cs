using Godot;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class CommanderList : VBoxContainer
{

	public override void _Ready()
	{
		Globals.MapScene.UpdateListeners.Add(Update);
	}

	private void Update()
	{
		var provence = Globals.MapScene.SelectedProvince;
		if (provence == null) return;
		
		GetChildren().OfType<Button>().ToList().ForEach(button =>
		{
			RemoveChild(button);
			button.QueueFree();
		});

		foreach (var commander in provence.Commanders.Where(commander => commander.Team == "Player"))
		{
			var button = new Button();
			button.Text = commander.Name;
			button.Pressed += () => Globals.MapScene.SelectedCommander = commander;
			AddChild(button);
		}
	}
}
