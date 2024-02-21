using Godot;

public partial class GUI : CanvasLayer
{
	private static GUI _instance;
	public static GUI Instance => _instance;
	public RecipePopup Popup => GetNode<RecipePopup>("%Popup");
	public CrafterGUI CrafterGui => GetNode<CrafterGUI>("%CrafterGUI");
	public ContainerGUI ContainerGui => GetNode<ContainerGUI>("%ContainerGUI");
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
}
