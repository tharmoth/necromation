using Godot;
using System;

public partial class ClickAndDrag : Node2D
{
	private Vector2 _start;
	private bool dragging = false;

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			if (!dragging)
			{
				GD.Print("Start Dragging");
				_start = ToLocal(GetGlobalMousePosition());
				dragging = true;
			}
			GD.Print("Dragging");
			QueueRedraw();
		}
		else
		{
			GD.Print("Stop Dragging");
			if (dragging)
			{
				dragging = false;
				QueueRedraw();
			}
		}
	}

	public override void _Draw()
	{
		base._Draw();
		if (!dragging) return;
		DrawRect(new Rect2(_start, ToLocal(GetGlobalMousePosition()) - _start), new Color(1, 1, 1, .5f));
	}
}
