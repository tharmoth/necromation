using Godot;
using System;
using System.Linq;
using Godot.Collections;
using Necromation;
using Necromation.gui;

public partial class IngrediantsPopup : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/IngrediantsPopupGui/IngrediantsPopup.tscn");

	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Label RecipeNameLabel => GetNode<Label>("%RecipeNameLabel");
	private VBoxContainer Rows => GetNode<VBoxContainer>("%Rows");
	private Label CraftingTimeLabel => GetNode<Label>("%CraftingTimeLabel");
	private DropShadowBorder DropShadowBorder => GetNode<DropShadowBorder>("%DropShadowBorder");
	
	/**************************************************************************
	 * State Data   													      *
	 **************************************************************************/
	private Recipe _recipe;
	private static Dictionary<Control, IngrediantsPopup> _popups = new();
	private Tween _tween;
	
	public static void Register(Recipe recipe, Control control)
	{
		ItemPopup.Unregister(control);
		var popup = DisplayPopup(recipe);
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
		Globals.FactoryScene.Gui.GetChildren().OfType<IngrediantsPopup>().ToList().ForEach(popup => popup.Kill());
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
	
	private static IngrediantsPopup DisplayPopup(Recipe recipe)
	{
		var popup = Scene.Instantiate<IngrediantsPopup>();
		popup._recipe = recipe;
		return popup;
	}

	public override void _Ready()
	{
		if (_recipe == null) throw new ArgumentException("Recipe popup recipe cannot be null when added to tree");
		Size = Vector2.Zero;
		
		_recipe.Products.First().Deconstruct(out var product, out var amount);
		RecipeNameLabel.Text = product + (amount == 1 ? "" : " x" + amount);
		CraftingTimeLabel.Text = _recipe.Time + "s Crafting Time";
		
		Rows.GetChildren().ToList().ForEach(node => node.Free());
		_recipe.Ingredients.ToList().ForEach(ingredient => AddRow(ingredient.Key, ingredient.Value));
		
		DropShadowBorder.DisableBlur();
		Modulate = Colors.Transparent;
		FactoryGUI.SnapToScreen(this);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		FactoryGUI.SnapToScreen(this);
	}

	private void AddRow(string name, int amount)
	{
		RichTextLabel label = new();

		var playerCount = Globals.PlayerInventory.CountItem(name);
		if (playerCount >= amount)
		{
			label.Text += amount + " x " + name;
		}
		else
		{
			label.Text += "[color=ef9698]" + playerCount + "/" + amount + " x " + name + "[/color]";
		}
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
		GetNode<VBoxContainer>("%Rows").AddChild(row);
	}
}
