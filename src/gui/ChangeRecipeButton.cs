using Godot;
using Necromation.gui;

public partial class ChangeRecipeButton : Button
{
	[Export] private Control _nodeToHide;
	public ICrafter _crafter;

	public override void _Pressed()
	{
		base._Pressed();
		GUI.Instance.Popup.DisplayPopup(_crafter);
		_nodeToHide.Visible = false;
	}
}
