using Godot;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class CommanderList : Container
{

	public override void _Ready()
	{
		Globals.MapScene.UpdateListeners.Add(Update);
	}

	private void Update()
	{
		// var provence = Globals.MapScene.SelectedProvince;
		//
		// if (provence == null) return;
		//
		// var commanders = provence.Commanders.Where(commander => commander.Team == "Player").ToList();
		// MapSquad.UpdateCommanderList(commanders, this);
	}
}
