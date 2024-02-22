using Godot;
using Necromation.gui;

public partial class GUI : CanvasLayer
{
	private static GUI _instance;
	public static GUI Instance => _instance;
	private RecipePopup Popup => GetNode<RecipePopup>("%Popup");
	private CrafterGUI CrafterGui => GetNode<CrafterGUI>("%CrafterGUI");
	private ContainerGUI ContainerGui => GetNode<ContainerGUI>("%ContainerGUI");
	public ProgressBar ProgressBar => GetNode<ProgressBar>("%ProgressBar");
	
	// Use _EnterTree to make sure the Singleton instance is avaiable in _Ready()
	public override void _EnterTree(){
		if(_instance != null){
			this.QueueFree(); // The Singleton is already loaded, kill this instance
		}
		_instance = this;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Input.IsActionJustPressed("open_technology")) ;
	}
	
	public bool IsAnyGUIOpen()
	{
		return Popup.Visible || CrafterGui.Visible || ContainerGui.Visible;
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
}
