using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation.map;
using Necromation.shared.gui;

public partial class MapGui : CanvasLayer, SceneGUI
{
	public Label SelectedLabel  => GetNode<Label>("%Label");
	private RecruitGUI RecruitGui => GetNode<RecruitGUI>("%RecruitGUI");
	private Control MainGui => GetNode<Control>("%MainGui");

	public bool GuiOpen => RecruitGui.Visible || GuiStack.Count > 0;
	
	public readonly Stack<Control> GuiStack = new();
	public static MapGui Instance { get; private set; }
	
	private Queue<Province> _battleQueue = new();

	public override void _EnterTree(){
		if(Instance != null){
			this.QueueFree(); // The Singleton is already loaded, kill this instance
		}
		Instance = this;
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (_battleQueue.Count > 0)
		{
			var province = _battleQueue.Dequeue();
			MapGlobals.SelectedProvince = province;
			MapToBattleButton.ChangeScene();
		}
		
		if (Input.IsActionJustPressed("close_gui")) CloseGui();
		if (Input.IsActionJustPressed("open_army_setup") && GuiStack.Count == 0) 
		{
			GuiStack.Push(ArmySetup.Display(MapGlobals.SelectedProvince));
			MainGui.Visible = false;
		}
		if (Input.IsActionJustPressed("open_recruit") && GuiStack.Count == 0)
		{
			RecruitGui.Visible = true;
			MainGui.Visible = false;
		}
		if (Input.IsActionJustPressed("open_map") && MapGlobals.SelectedProvince.Owner == "Player") MapToFactoryButton.ChangeScene();
		if (Input.IsActionJustPressed("end_turn")) EndTurn();
	}

	private void EndTurn()
	{
		// Create a copy of the list as it may be modified during the loop
		var listeners = MapGlobals.TurnListeners.ToList();
		listeners.ForEach(listener => listener());
		
		MapGlobals.TileMap.GetProvinces().ForEach(province =>
		{
			var teams = province.Commanders.GroupBy(leader => leader.Team).ToList();
			
			if (teams.Count > 1)
			{
				_battleQueue.Enqueue(province);
			}
		});

		MapGlobals.UpdateListeners.ForEach(listener => listener());
	}

	public void CloseGui()
	{
		GuiStack.TryPop(out var gui);
		gui?.QueueFree();
		
		RecruitGui.Visible = false;
		MainGui.Visible = true;
	}
}
