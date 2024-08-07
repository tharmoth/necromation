using System.Collections.Generic;
using Godot;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.character;

public partial class MapGui : CanvasLayer
{
	public static MapGui Instance { get; private set; }
	public readonly Stack<Control> GuiStack = new();
	public bool GuiOpen => RecruitGui.Visible || GuiStack.Count > 0;
	
	public Label SelectedLabel  => GetNode<Label>("%Label");
	private Button FactoryButton => GetNode<Button>("%FactoryButton");
	private Button ArmyButton => GetNode<Button>("%ArmyButton");
	private RecruitGUI RecruitGui => GetNode<RecruitGUI>("%RecruitGUI");
	private Control MainGui => GetNode<Control>("%MainGui");
	
	public override void _EnterTree(){
		if(Instance != null){
			QueueFree(); // The Singleton is already loaded, kill this instance
		}
		Instance = this;
	}
	
	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed("close_gui")) CloseGui();
		if (inputEvent.IsActionPressed("open_map")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
		if (inputEvent.IsActionPressed("open_help"))
		{
			if (GuiStack.Count > 0 && GuiStack.Peek() is HelpGui)
				CloseGui();
			else
				HelpGui.Display(false);
		}
	}
	
	public void Open(Control gui)
	{
		while (GuiStack.Count > 0)
		{
			CloseGui();
		}
		GuiStack.Push(gui);
		MusicManager.Play("ui_open");
		AddChild(gui);
	}

	public void CloseGui()
	{
		if (GuiStack.Count == 0) SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
		GuiStack.TryPop(out var gui);
		gui?.QueueFree();
		
		RecruitGui.Visible = false;
		MainGui.Visible = true;
	}
}
