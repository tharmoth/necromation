using Godot;
using Necromation;
using Necromation.gui;

public partial class ChangeRecipeButton : Button
{
	[Export] private Control _nodeToHide;
	public ICrafter Crafter;

	public override void _Pressed()
	{
		base._Pressed();
		Globals.FactoryScene.Gui.Display(Crafter);
		_nodeToHide.Visible = false;
	}
}
