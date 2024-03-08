using System.Linq;
using Godot;
using Necromation;
using Necromation.gui;
using Necromation.map;
using Necromation.sk;

public partial class FactoryGUI : CanvasLayer
{
	private Label FpsLabel => GetNode<Label>("%FpsLabel");
	private ContainerGUI ContainerGui => GetNode<ContainerGUI>("%ContainerGUI");
	private ProgressBar ProgressBar => GetNode<ProgressBar>("%ProgressBar");
	private Label AttackLabel => GetNode<Label>("%AttackLabel");
	
	private TechGUI TechGui => GetNode<TechGUI>("%TechGUI");
	private AudioStreamPlayer TechnologyCompleteAudio => GetNode<AudioStreamPlayer>("%TechnologyCompleteAudio");
	private AudioStreamPlayer BuildingRemovedAudio => GetNode<AudioStreamPlayer>("%BuildingRemovedAudio");

	private Control _openGui;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Globals.FactoryScene is FactoryScene { AttackTimer: not null } main)
		{
			AttackLabel.Visible = true;
			AttackLabel.Text = $"You are being attacked at {main.AttackProvince.Name}!\nNext attack in {main.AttackTimer.TimeLeft:0.0} seconds";
		}
		else
		{
			AttackLabel.Visible = false;
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		
		base._UnhandledInput(inputEvent);
		if (!inputEvent.IsPressed()) return;
		
		if (Input.IsActionJustPressed("close_gui")) CloseGui();
		if (Input.IsActionJustPressed("open_technology"))
		{
			if (TechGui.Visible)
			{
				TechGui.Visible = false;
			}
			else
			{
				CloseGui();
			}
		}

		if (Input.IsActionJustPressed("open_inventory")) InventoryGUI.Display(Globals.PlayerInventory);
		if (Input.IsActionJustPressed("open_map")) SceneManager.ChangeToScene(SceneManager.SceneEnum.Map);
		if (Input.IsActionPressed("open_config")) ConfigurationGui.Display();

		if (Input.IsActionJustPressed("save")) SKSaveLoad.SaveGame(this);
		if (Input.IsActionJustPressed("load")) SKSaveLoad.LoadGame(this);
		
	}
	
	public void OpenGui(Control gui)
	{
		var open = _openGui;
		CloseGui();
		if (IsInstanceValid(open) && open.GetType() == gui.GetType()) return;
		_openGui = gui;
		AddChild(gui);
	}
	
	public void CloseGui()
	{
		ContainerGui.Visible = false;
		TechGui.Visible = false;
		if (IsInstanceValid(_openGui)) _openGui?.QueueFree();
		_openGui = null;
	}
	
	public bool IsAnyGuiOpen()
	{
		return ContainerGui.Visible || TechGui.Visible || _openGui != null;
	}
	
	public void Display(Inventory to, Inventory from, string title)
	{
		CloseGui();
		ContainerGui.Display(to, from, title);
	}

	public void SetProgress(double value)
	{
		ProgressBar.Visible = true;
		ProgressBar.Value = value * 100;
		ProgressBar.Visible = value is < 1 and > 0;
	}
	
	public void TechnologyComplete()
	{
		TechnologyCompleteAudio.Play();
	}
	
	public void BuildingRemoved()
	{
		BuildingRemovedAudio.Play();
	}
	
	public void ToggleFps()
	{
		FpsLabel.Visible = !FpsLabel.Visible;
	}
		
	/*
	 * Adjusts the position of the popup so that it is always visible on the screen
	 */
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
