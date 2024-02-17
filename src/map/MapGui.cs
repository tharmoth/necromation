using Godot;
using System.Linq;
using Necromation.map;

public partial class MapGui : CanvasLayer
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
}
