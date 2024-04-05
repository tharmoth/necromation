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
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/ItemPopup/ItemPopup.tscn");
	
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private RichTextLabel Label => GetNode<RichTextLabel>("%Label");
	private Container Rows => GetNode<Container>("%Rows");
	
	/**************************************************************************
	 * State Data   													      *
	 **************************************************************************/
	private static Dictionary<Control, ItemPopup> _popups = new();
	private Tween _tween;
	
	public static void Register(Control control, string popupText, List<string> items = null)
	{
		if (_popups.ContainsKey(control)) Unregister(control);
		
		var popup = Scene.Instantiate<ItemPopup>();
		popup.Init(popupText, items);
		control.MouseEntered += popup.Display;
		control.MouseExited += popup.Fade;
		control.VisibilityChanged += popup.Kill;
		control.TreeExited += popup.Kill;
		_popups.Add(control, popup);
	}

	public static void Unregister(Control control)
	{
		if (!_popups.ContainsKey(control)) return;
		var popup = _popups[control];
		_popups[control].Kill();
		_popups.Remove(control);
		control.MouseEntered -= popup.Display;
		control.MouseExited -= popup.Fade;
		control.VisibilityChanged -= popup.Kill;
		control.TreeExited -= popup.Kill;
	}
	
	/// <summary>
	/// Constructor for the ItemListPopup.
	/// </summary>
	/// <param name="popupText"> Text to display </param>
	private void Init(string popupText, List<string> items)
	{
		Label.Text = popupText;
		items?.ToList().ForEach(AddRow);
		ZIndex = 1000;
	}
	
	/// <summary>
	/// Sets the popup to visible and adds it to the GUI.
	/// </summary>
	private void Display()
	{
		Globals.FactoryScene.Gui.GetChildren().OfType<ItemPopup>().ToList().ForEach(popup => popup.Kill());
		if (!IsInstanceValid(this)) return;
		Modulate = Colors.White;
		
		Globals.FactoryScene.Gui.AddChild(this);
		
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 1, .15f);
	}

	/// <summary>
	/// Sets the popup to invisible and removes it from the GUI.
	/// </summary>
	private void Kill()
	{
		if (!IsInstanceValid(this)) return;
		_tween?.Kill();
		_tween = null;
		Modulate = Colors.Transparent;
		if (GetParent() != null) GetParent().RemoveChild(this);
	}

	/// <summary>
	/// Fades the popup to invisible over time and then removes it from the GUI.
	/// </summary>
	private void Fade()
	{
		if (!IsInstanceValid(this)) return;
		_tween?.Kill();
		_tween = Globals.Tree.CreateTween();
		_tween.TweenProperty(this, "modulate:a", 0, .15f);
		_tween.TweenCallback(Callable.From(() =>
		{
			if (GetParent() != null) GetParent().RemoveChild(this);
		}));
	}
	
	private void AddRow(string name)
	{
		RichTextLabel label = new();

		label.Text += name;
		label.AutowrapMode = TextServer.AutowrapMode.Off;
		label.BbcodeEnabled = true;
		label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		label.SizeFlagsVertical = SizeFlags.Fill;
		label.FitContent = true;
		
		var texture = new TextureRect();
		texture.Texture = Database.Instance.GetTexture(name);
		texture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		texture.CustomMinimumSize = new Vector2(32, 32);
		
		var row = new HBoxContainer();
		row.AddChild(texture);
		row.AddChild(label);
		Rows.AddChild(row);
	}

	public override void _Ready()
	{
		base._Ready();
		
		Size = Vector2.Zero;

		// Call deffered to wait for RichTextLabel to update
		CallDeferred("Update");
	}
	
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		Update();
	}
	
	private void Update()
	{
		if (GetParent() == null) return;
		FactoryGUI.SnapToScreen(this);
	}
}
