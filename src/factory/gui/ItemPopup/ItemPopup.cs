using Godot;
using System;
using System.Linq;
using Necromation;

public partial class ItemPopup : PanelContainer
{
	private DropShadowBorder DropShadowBorder => GetNode<DropShadowBorder>("%DropShadowBorder");
	private Label Label => GetNode<Label>("%Label");
	private string _itemType;
	
	public static void Register(string itemType, Control control)
	{
		ItemPopup popup = null;
		control.MouseEntered += () =>
		{
			if (IsInstanceValid(popup)) return;
			popup = DisplayPopup(itemType);
		};
		control.MouseExited += () =>
		{
			if (!IsInstanceValid(popup)) return;
			var tween = Globals.Tree.CreateTween();
			popup.DropShadowBorder.DisableBlur();
			tween.TweenProperty(popup, "modulate:a", 0, .15f);
			tween.TweenCallback(Callable.From(() =>
			{
				if (IsInstanceValid(popup)) popup.QueueFree();
			}));
		};
		control.VisibilityChanged += () =>
		{
			if (IsInstanceValid(popup)) popup.QueueFree();
		};
		control.TreeExited += () =>
		{
			if (IsInstanceValid(popup)) popup.QueueFree();
		};
	}
	
	private static ItemPopup DisplayPopup(string itemType)
	{
		Globals.FactoryScene.Gui.GetChildren().OfType<IngrediantsPopup>().ToList().ForEach(popup => popup.QueueFree());
		var popup =GD
			.Load<PackedScene>("res://src/factory/gui/ItemPopup/item_popup.tscn")
			.Instantiate<ItemPopup>();
		popup.Init(itemType);
		
		// Normally we want to use OpenGui, but popups don't go on the GUI stack.
		Globals.FactoryScene.Gui.AddChild(popup);
		return popup;
	}

	private void Init(string itemType)
	{
		_itemType = itemType;
	}

	public override void _Ready()
	{
		base._Ready();
		Label.Text = _itemType;
		FactoryGUI.SnapToScreen(this);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		FactoryGUI.SnapToScreen(this);
	}
}
