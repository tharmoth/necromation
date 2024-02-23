using Godot;
using System.Linq;
using Necromation.map;

public partial class CommanderList : VBoxContainer
{

	public override void _Ready()
	{
		MapGlobals.UpdateListeners.Add(Update);
	}

	private void Update()
	{
		var provence = MapGlobals.SelectedProvince;
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
			button.Pressed += () => MapGlobals.SelectedCommander = commander;
			AddChild(button);
		}
	}
}
