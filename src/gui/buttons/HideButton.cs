using Godot;
using Godot.Collections;

public partial class HideButton : Button
{
	[Export] private Control _nodeToHide;
	[Export] private Array<Control> _nodesToShow = new();

	public override void _Pressed()
	{
		base._Pressed();
		_nodeToHide.Visible = false;
		foreach (var node in _nodesToShow)
		{
			node.Visible = true;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (!Input.IsActionPressed("close_gui") || !_nodeToHide.Visible) return;
		
		_nodeToHide.Visible = false;
		
		foreach (var node in _nodesToShow) {
			node.Visible = true;
		}
	}
}
