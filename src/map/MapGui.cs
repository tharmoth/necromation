using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;
using Necromation.shared.gui;

public partial class MapGui : CanvasLayer, SceneGUI
{
	public Label SelectedLabel  => GetNode<Label>("%Label");
	private Button _factoryButton => GetNode<Button>("%FactoryButton");
	private Button _armyButton => GetNode<Button>("%ArmyButton");
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

	public override void _Ready()
	{
		base._Ready();
		MapGlobals.UpdateListeners.Add(Update);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (_battleQueue.Count > 0)
		{
			var province = _battleQueue.Dequeue();
			MapGlobals.SelectedProvince = province;
			SceneManager.ChangeToScene(SceneManager.SceneEnum.Battle);
		}
		
		if (Input.IsActionJustPressed("close_gui")) CloseGui();
		if (Input.IsActionJustPressed("open_army_setup") && GuiStack.Count == 0 && MapGlobals.SelectedProvince.Owner == "Player") 
		{
			GuiStack.Push(ArmySetup.Display(MapGlobals.SelectedProvince));
			MainGui.Visible = false;
		}
		if (Input.IsActionJustPressed("open_recruit") && GuiStack.Count == 0 && MapGlobals.SelectedProvince.Owner == "Player")
		{
			RecruitGui.Visible = true;
			MainGui.Visible = false;
		}
		if (Input.IsActionJustPressed("open_map") && MapGlobals.SelectedProvince.Owner == "Player") SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
		if (Input.IsActionJustPressed("end_turn")) EndTurn();
		if (Input.IsActionJustPressed("add_squad"))
		{
			var commander = new Commander(MapGlobals.SelectedProvince, "Player");
			commander.GlobalPosition = MapGlobals.SelectedProvince.GlobalPosition;
			MapGlobals.SelectedProvince.Commanders.Add(commander);
			Globals.MapScene.AddChild(commander);
			CloseGui();
			MainGui.Visible = false;
			GuiStack.Push(ArmySetup.Display(MapGlobals.SelectedProvince));
			MapGlobals.UpdateListeners.ForEach(listener => listener());
		};
	}

	private void EndTurn()
	{
		// Create a copy of the list as it may be modified during the loop
		var listeners = MapGlobals.TurnListeners.ToList();
		listeners.ForEach(listener => listener());

		Battle();

		MapGlobals.UpdateListeners.ForEach(listener => listener());
	}

	public void Battle()
	{
		Globals.MapScene.TileMap.GetProvinces().ForEach(province =>
		{
			var teams = province.Commanders.GroupBy(leader => leader.Team).ToList();
			
			if (teams.Count > 1)
			{
				_battleQueue.Enqueue(province);
			}
		});
	}

	public void CloseGui()
	{
		GuiStack.TryPop(out var gui);
		gui?.QueueFree();
		
		RecruitGui.Visible = false;
		MainGui.Visible = true;
	}

	private void Update()
	{
		var provence = MapGlobals.SelectedProvince;

		var units = new Dictionary<string, int>();

		foreach (var (unit, count) in provence.Commanders.SelectMany(commander => commander.Units.Items).ToList())
		{
			units.TryGetValue(unit, out var currentCount);
			units[unit] = currentCount + count;
		}

		var unitString = "";
		foreach (var (unit, count) in units)
		{
			unitString += unit + " x" + count + "\n";
		}
		if (unitString == "") unitString = "no units\n";
		MapGui.Instance.SelectedLabel.Text = provence.Name + "\n" + unitString;
		
		_factoryButton.Disabled = provence.Owner != "Player";
		_armyButton.Disabled = provence.Owner != "Player";
	}
}
