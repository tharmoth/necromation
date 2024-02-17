using Godot;
using Godot.Collections;

public partial class ShowButton : Button
{
	[Export] private Control _nodeToShow;
	[Export] private Array<Control> _nodesToHide = new();

	public override void _Pressed()
	{
		base._Pressed();
		_nodeToShow.Visible = true;
		foreach (var node in _nodesToHide)
		{
			node.Visible = false;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (!(Input.IsActionPressed("close_gui") && _nodeToShow.Visible)) return;
		
		_nodeToShow.Visible = false;
		foreach (var node in _nodesToHide)
		{
			node.Visible = true;
		}
	}
}
