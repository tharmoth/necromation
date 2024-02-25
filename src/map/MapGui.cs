using Godot;
using System.Linq;
using Necromation.map;
using Necromation.shared.gui;

public partial class MapGui : CanvasLayer, SceneGUI
{
	public Label SelectedLabel  => GetNode<Label>("%Label");
	public ArmySetup ArmySetup  => GetNode<ArmySetup>("%ArmySetup");
	private RecruitGUI _recruitGUI => GetNode<RecruitGUI>("%RecruitGUI");

	public bool GuiOpen
	{
		get
		{
			return ArmySetup.Visible || _recruitGUI.Visible;
		}
	}

	private static MapGui _instance;
	public static MapGui Instance => _instance;
	public override void _EnterTree(){
		if(_instance != null){
			this.QueueFree(); // The Singleton is already loaded, kill this instance
		}
		_instance = this;
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
	}

	public void CloseGui()
	{
		_currentGui?.QueueFree();
		_currentGui = null;
	}
}
