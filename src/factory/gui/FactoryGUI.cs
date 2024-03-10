using System.Linq;
using Godot;
using Necromation;
using Necromation.gui;
using Necromation.map;
using Necromation.sk;

public partial class FactoryGUI : CanvasLayer
{
	/************************************************************************
	 * Child Accessors 													    *
	 ************************************************************************/
	private Label FpsLabel => GetNode<Label>("%FpsLabel");
	private ProgressBar ProgressBar => GetNode<ProgressBar>("%ProgressBar");
	private Container ProgressPanel => GetNode<Container>("%ProgressPanel");
	private Label AttackLabel => GetNode<Label>("%AttackLabel");
	private ColorRect TopBar => GetNode<ColorRect>("%TopBar");
	private ColorRect BottomBar => GetNode<ColorRect>("%BottomBar");
	private AudioStreamPlayer TechnologyCompleteAudio => GetNode<AudioStreamPlayer>("%TechnologyCompleteAudio");
	private AudioStreamPlayer BuildingRemovedAudio => GetNode<AudioStreamPlayer>("%BuildingRemovedAudio");
	private AudioStreamPlayer GuiOpenedAudio => GetNode<AudioStreamPlayer>("%GuiOpenedAudio");
	private AudioStreamPlayer GuiClosedAudio => GetNode<AudioStreamPlayer>("%GuiClosedAudio");
	public GridContainer HotBar => GetNode<GridContainer>("%HotBar");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private Control _openGui;
	
	/***************************************************************************
	 * Properties  														       *
	 ***************************************************************************/
	public bool IsAnyGuiOpen() => _openGui != null;

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		base._UnhandledInput(inputEvent);
		if (!inputEvent.IsPressed()) return;
		if (Input.IsActionJustPressed("close_gui")) CloseGui();

		if (Input.IsActionJustPressed("open_technology"))
		{
			if (_openGui is TechGUI)
				CloseGui();
			else
				TechGUI.Display();
		}

		if (Input.IsActionJustPressed("open_inventory"))
		{
			if (_openGui is InventoryGUI)
				CloseGui();
			else
				InventoryGUI.Display(Globals.PlayerInventory);
		}

		if (Input.IsActionPressed("open_config"))
		{
			if (_openGui is ConfigurationGui)
				CloseGui();
			else
				ConfigurationGui.Display();
		}
		
		if (Input.IsActionJustPressed("open_map")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Map);
		if (Input.IsActionJustPressed("save")) SKSaveLoad.SaveGame(this);
		if (Input.IsActionJustPressed("load")) SKSaveLoad.LoadGame(this);
	}
	
	/***************************************************************************
	 * Public API Methods  													   *
	 ***************************************************************************/
	public void OpenGui(Control gui)
	{
		CloseGui();
		_openGui = gui;
		GuiOpenedAudio.Play();
		AddChild(gui);
	}
	
	public void CloseGui()
	{
		if (IsInstanceValid(_openGui))
		{
			_openGui?.QueueFree();
			GuiClosedAudio.Play();
		}
		_openGui = null;
	}
		
	public void ToggleCinematicMode()
	{
		if (Config.CinematicMode)
		{
			TopBar.Visible = true;
			BottomBar.Visible = true;
		}
		else
		{
			TopBar.Visible = false;
			BottomBar.Visible = false;
		}
	}

	public void SetProgress(double value)
	{
		ProgressBar.Value = value * 100;
		ProgressPanel.Visible = value is < 1 and > 0;
	}
	
	public void TechnologyComplete()
	{
		// Audio Should be moved to MusicManager
		TechnologyCompleteAudio.Play();
	}
	public void PlayBuildingRemovedAudio()
	{
		BuildingRemovedAudio.Play();
	}
	
	public void ToggleFps()
	{
		FpsLabel.Visible = !FpsLabel.Visible;
	}
	
	/***************************************************************************
	 * Static Utility Methods  												   *
	 ***************************************************************************/
	// Adjusts the position of the popup so that it is always visible on the screen
	public static void SnapToScreen(Control node)
	{
		node.ResetSize();
		
		node.GlobalPosition = node.GetViewport().GetMousePosition() + new Vector2(40, 0);
		
		// Ensure the PopupMenu is not partially off-screen
		var screenSize = node.GetViewportRect().Size;
		
		// Check if the PopupMenu exceeds the right edge of the screen move it to the left of the cursor
		if (node.GlobalPosition.X + node.Size.X > screenSize.X)
		{
			node.GlobalPosition = new Vector2(node.GetViewport().GetMousePosition().X - node.Size.X - 40, node.GlobalPosition.Y);
		}
		
		// Check if the PopupMenu exceeds the bottom edge of the screenmove it to the top of the cursor
		if (node.GlobalPosition.Y + node.Size.Y > screenSize.Y)
		{
			node.GlobalPosition = new Vector2(node.GlobalPosition.X, screenSize.Y - node.Size.Y);
		}
	}
}
