using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class ItemPopup : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/ItemPopup/item_popup.tscn");
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private DropShadowBorder DropShadowBorder => GetNode<DropShadowBorder>("%DropShadowBorder");
	private Label Label => GetNode<Label>("%Label");
	
	/**************************************************************************
	 * State Data   													      *
	 **************************************************************************/
	private string _itemType;
	private static Dictionary<Control, ItemPopup> _popups = new();
	private Tween _tween;
	
	public static void Register(string itemType, Control control)
	{
		if (_popups.ContainsKey(control)) Unregister(control);
		
		var popup = GeneratePopup(itemType);
		control.MouseEntered += () =>
		{
			popup.Display();
		};
		control.MouseExited += popup.Fade;
		control.VisibilityChanged += popup.Kill;
		control.TreeExited += popup.Kill;
		_popups.Add(control, popup);
	}

	public static void Unregister(Control control)
	{
		if (!_popups.ContainsKey(control)) return;
		_popups[control].Kill();
		_popups.Remove(control);
	}
	
	private void Display()
	{
		Globals.FactoryScene.Gui.GetChildren().OfType<ItemPopup>().ToList().ForEach(popup => popup.Kill());
		if (!IsInstanceValid(this)) return;
		Modulate = Colors.White;
		
		Globals.FactoryScene.Gui.AddChild(this);
		FactoryGUI.SnapToScreen(this);
		
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 1, .15f);
	}

	private void Kill()
	{
		if (!IsInstanceValid(this)) return;
		_tween?.Kill();
		_tween = null;
		Modulate = Colors.Transparent;
		if (GetParent() != null) GetParent().RemoveChild(this);
	}

	private void Fade()
	{
		if (!IsInstanceValid(this)) return;
		DropShadowBorder.DisableBlur();
		_tween?.Kill();
		_tween = Globals.Tree.CreateTween();
		_tween.TweenProperty(this, "modulate:a", 0, .15f);
		_tween.TweenCallback(Callable.From(() =>
		{
			if (GetParent() != null) GetParent().RemoveChild(this);
		}));
	}
	
	private static ItemPopup GeneratePopup(string itemType)
	{
		var popup = Scene.Instantiate<ItemPopup>();
		popup.Init(itemType);
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
