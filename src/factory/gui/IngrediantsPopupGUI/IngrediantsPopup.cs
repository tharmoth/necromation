using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class IngrediantsPopup : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/IngrediantsPopupGUI/ingrediants_popup.tscn");

	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Label RecipeNameLabel => GetNode<Label>("%RecipeNameLabel");
	private VBoxContainer Rows => GetNode<VBoxContainer>("%Rows");
	private Label CraftingTimeLabel => GetNode<Label>("%CraftingTimeLabel");
	private DropShadowBorder DropShadowBorder => GetNode<DropShadowBorder>("%DropShadowBorder");
	
	
	public static void Register(Recipe recipe, Control control)
	{
		IngrediantsPopup popup = null;
		control.MouseEntered += () =>
		{
			if (IsInstanceValid(popup)) return;
			popup = DisplayPopup(recipe);
		};
		control.MouseExited += () =>
		{
			if (!IsInstanceValid(popup)) return;
			var tween = Globals.Tree.CreateTween();
			popup.DropShadowBorder.DisableBlur();
			tween.TweenProperty(popup, "modulate:a", 0, .15f);
			tween.TweenCallback(Callable.From(() =>
			{
				if (!IsInstanceValid(popup)) return;
				popup.QueueFree();
			}));
		};
		control.VisibilityChanged += () =>
		{
			if (!IsInstanceValid(popup)) return;
			popup.QueueFree();
		};
		control.TreeExited += () =>
		{
			if (!IsInstanceValid(popup)) return;
			popup.QueueFree();
		};
	}
	
	public static IngrediantsPopup DisplayPopup(Recipe recipe)
	{
		Globals.FactoryScene.Gui.GetChildren().OfType<IngrediantsPopup>().ToList().ForEach(popup => popup.QueueFree());
		var popup = Scene.Instantiate<IngrediantsPopup>();
		popup._recipe = recipe;
		Globals.FactoryScene.Gui.AddChild(popup);
		return popup;
	}

	private Recipe _recipe;
	

	public override void _Ready()
	{
		if (_recipe == null) throw new ArgumentException("Recipe popup recipe cannot be null when added to tree");
		Size = Vector2.Zero;
		
		_recipe.Products.First().Deconstruct(out var product, out var amount);
		RecipeNameLabel.Text = product + (amount == 1 ? "" : " x" + amount);
		CraftingTimeLabel.Text = _recipe.Time + "s Crafting Time";
		
		Rows.GetChildren().ToList().ForEach(node => node.Free());
		_recipe.Ingredients.ToList().ForEach(ingredient => AddRow(ingredient.Key, ingredient.Value));
		
		var tween = CreateTween();
		DropShadowBorder.DisableBlur();
		Modulate = Colors.Transparent;
		tween.TweenProperty(this, "modulate:a", 1, .15f);
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
