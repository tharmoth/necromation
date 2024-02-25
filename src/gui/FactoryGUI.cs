using Godot;
using Necromation;
using Necromation.gui;
using Necromation.shared.gui;

public partial class FactoryGUI : CanvasLayer, SceneGUI
{
	private static FactoryGUI _instance;
	public static FactoryGUI Instance => _instance;
	private RecipePopup Popup => GetNode<RecipePopup>("%Popup");
	private CrafterGUI CrafterGui => GetNode<CrafterGUI>("%CrafterGUI");
	private ContainerGUI ContainerGui => GetNode<ContainerGUI>("%ContainerGUI");
	private ProgressBar ProgressBar => GetNode<ProgressBar>("%ProgressBar");
	private FactoryToMapButton FactoryToMapButton => GetNode<FactoryToMapButton>("%FactoryToMapButton");
	
	private TechGUI TechGui => GetNode<TechGUI>("%TechGUI");
	
	// Use _EnterTree to make sure the Singleton instance is avaiable in _Ready()
	public override void _EnterTree(){
		if(_instance != null){
			this.QueueFree(); // The Singleton is already loaded, kill this instance
		}
		_instance = this;
	}

	private Control _openGui; 

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Input.IsActionPressed("close_gui")) CloseGui();
		if (Input.IsActionJustPressed("open_technology")) TechGui.Display();
		if (Input.IsActionJustPressed("open_inventory") && _openGui != null) CloseGui();
		else if (Input.IsActionJustPressed("open_inventory")) _openGui = InventoryGUI.Display(Globals.PlayerInventory);
		if (Input.IsActionJustPressed("open_map")) FactoryToMapButton.ChangeScene();
	}
	
	public void CloseGui()
	{
		_openGui?.QueueFree();
		_openGui = null;
	}
	
	public bool IsAnyGuiOpen()
	{
		return Popup.Visible || CrafterGui.Visible || ContainerGui.Visible || TechGui.Visible || _openGui != null;
	}

	public void Display(ICrafter crafter)
	{
		Popup.DisplayPopup(crafter);
	}
	
	public void Display(Inventory to, ICrafter crafter)
	{
		CrafterGui.Display(to, crafter);
	}
	
	public void Display(Inventory to, Inventory from, string title)
	{
		ContainerGui.Display(to, from, title);
	}

	public void SetProgress(double value)
	{
		ProgressBar.Visible = true;
		ProgressBar.Value = value * 100;
		ProgressBar.Visible = value is < 1 and > 0;
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
