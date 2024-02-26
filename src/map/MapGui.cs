using Godot;
using System.Linq;
using Necromation.map;
using Necromation.shared.gui;

public partial class MapGui : CanvasLayer, SceneGUI
{
	public Label SelectedLabel  => GetNode<Label>("%Label");
	private RecruitGUI RecruitGui => GetNode<RecruitGUI>("%RecruitGUI");
	private Control MainGui => GetNode<Control>("%MainGui");

	public bool GuiOpen => RecruitGui.Visible || _currentGui != null;

	public static MapGui Instance { get; private set; }

	public override void _EnterTree(){
		if(Instance != null){
			this.QueueFree(); // The Singleton is already loaded, kill this instance
		}
		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();
		GetNode<Button>("%NextTurn").Pressed += () =>
		{
			// Create a copy of the list as it may be modified during the loop
			var listeners = MapGlobals.TurnListeners.ToList();
			listeners.ForEach(listener => listener());
			MapGlobals.UpdateListeners.ForEach(listener => listener());
		};
	}

	private Control _currentGui;
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Input.IsActionJustPressed("close_gui")) CloseGui();
		if (Input.IsActionJustPressed("open_army_setup") && _currentGui == null) _currentGui = ArmyLayout.Display(MapGlobals.SelectedProvince);
		if (Input.IsActionJustPressed("open_recruit") && _currentGui == null) RecruitGui.Visible = true;
	}

	public void CloseGui()
	{
		_currentGui?.QueueFree();
		_currentGui = null;
		
		RecruitGui.Visible = false;
		MainGui.Visible = true;
	}
}
