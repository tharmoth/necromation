using Godot;
using System;
using Necromation.sk;

public partial class MainMenu : Control
{
	/************************************************************************
	 * Child Accessors 													    *
	 ************************************************************************/
	private ColorRect TransitionFade => GetNode<ColorRect>("%TransitionFade");
	
	public override void _UnhandledInput(InputEvent inputEvent)
	{
		base._UnhandledInput(inputEvent);
		if (!inputEvent.IsPressed()) return;
		if (inputEvent.IsActionPressed("new_game"))
		{
			SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
			QueueFree();
		}
		if (inputEvent.IsActionPressed("load"))
		{
			FactoryScene.ShouldLoad = true;
			SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory);
			QueueFree();
		}
		if (inputEvent.IsActionPressed("exit_game")) GetTree().Quit();
	}
}
