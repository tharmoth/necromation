using Godot;
using Necromation.gui;

public partial class ChangeRecipeButton : Button
{
	[Export] private Control _nodeToHide;
	public ICrafter Crafter;

	public override void _Pressed()
	{
		base._Pressed();
		GUI.Instance.Display(Crafter);
		_nodeToHide.Visible = false;
	}
}
