using System.Collections.Generic;
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
	public GridContainer HotBar => GetNode<GridContainer>("%HotBar");

	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private readonly Stack<Control> _guiStack = new();
	
	/***************************************************************************
	 * Properties  														       *
	 ***************************************************************************/
	public bool IsAnyGuiOpen() => _guiStack.Count != 0;

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		base._UnhandledInput(inputEvent);
		if (!inputEvent.IsPressed()) return;
		if (Input.IsActionJustPressed("close_gui")) CloseGui();

		if (Input.IsActionJustPressed("open_technology"))
		{
			if (_guiStack.Count > 0 && _guiStack.Peek() is TechGUI)
				CloseGui();
			else
				TechGUI.Display();
		}

		if (Input.IsActionJustPressed("open_inventory"))
		{
			if (_guiStack.Count > 0 && _guiStack.Peek() is InventoryGui)
				CloseGui();
			else
				InventoryGui.Display(Globals.PlayerInventory);
		}

		if (Input.IsActionPressed("open_config"))
		{
			if (_guiStack.Count > 0 && _guiStack.Peek() is ConfigurationGui)
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
	/// <summary>
	/// Opens a GUI without closing other GUIs.
	/// </summary>
	/// <param name="gui">The new GUI to add to the stack</param>
	public void Push(Control gui)
	{
		_guiStack.Push(gui);
		MusicManager.Play("ui_open");
		AddChild(gui);
	}
	
	/// <summary>
	/// Opens a GUI and closes all other GUIs.
	/// </summary>
	/// <param name="gui">The new GUI to open</param>
	public void Open(Control gui)
	{
		while (_guiStack.Count > 0)
		{
			CloseGui();
		}
		_guiStack.Push(gui);
		MusicManager.Play("ui_open");
		AddChild(gui);
	}
	
	public void CloseGui()
	{
		if (_guiStack.Count == 0) return;
		var topGui = _guiStack.Pop();
		if (!IsInstanceValid(topGui)) return;
		topGui.QueueFree();
		MusicManager.Play("ui_close");
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
