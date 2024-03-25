using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.gui;

public partial class TechPanel : PanelContainer
{
	private Label TitleLabel => GetNode<Label>("%TitleLabel");
	private Label CostLabel => GetNode<Label>("%CostLabel");
	private Label DescriptionLabel => GetNode<Label>("%DescriptionLabel");
	private Label UnlocksLabel => GetNode<Label>("%UnlocksLabel");
	private RichTextLabel PrerequisitesLabel => GetNode<RichTextLabel>("%PrerequisitesLabel");
	private Container EffectsRow => GetNode<Container>("%EffectsRow");
	private TextureRect CostTexture => GetNode<TextureRect>("%CostTexture");
	private Button SelectButton => GetNode<Button>("%SelectButton");

	public Technology Tech;

	public override void _Ready()
	{
		base._Ready();
		
		if (Tech == null) return;
		if (Tech.Researched)
		{
			QueueFree();
			return;
		}
		
		TitleLabel.Text = Tech.Name;
		CostTexture.Texture = Database.Instance.GetTexture(Tech.Ingredients.First());
		CostLabel.Text = Tech.Count.ToString();
		DescriptionLabel.Text = Tech.Description;

		UnlocksLabel.Text = Tech.Unlocks.Count > 0
			? string.Join(", ", Tech.Unlocks)
			: "No unlocks";
		
		Tech.Unlocks.ToList().ForEach(unlock =>
		{
			if (!Database.Instance.Recipes.Select(recipe => recipe.Name).Contains(unlock))
			{
				GD.PrintErr("Missing recipe for " + unlock);
				return;
			}

			var recipe = Database.Instance.Recipes.First(recipe => recipe.Name == unlock);
			var texture = new TextureRect();
			texture.Texture = recipe.GetIcon();
			texture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			texture.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
			texture.CustomMinimumSize = new Vector2(64, 64);
			EffectsRow.AddChild(texture);
			IngrediantsPopup.Register(recipe, texture);
		});

		PrerequisitesLabel.Text = Tech.Prerequisites.Count > 0
			? "[color=red]" + string.Join("[/color], [color=red]", Tech.Prerequisites) + "[/color]"
			: "No prerequisites";

		SelectButton.Pressed += () =>
		{
			if (Globals.Souls < Tech.Count) return;
			Globals.Souls -= Tech.Count;
			Globals.CurrentTechnology = Tech;
			Tech.Progress = Tech.Count;
			QueueFree();
		};

		Globals.SoulListeners.Add(Update);
		Update();
	}

	private void Update()
	{
		SelectButton.Disabled = false;
		
		if (Tech.Count > Globals.Souls) SelectButton.Disabled = true;

		foreach (var prerequisite in Tech.Prerequisites)
		{
			var researched = Database.Instance.Technologies
				.Where(tech => tech.Researched)
				.Select(tech => tech.Name)
				.Any(name => name == prerequisite);
			if (!researched)
			{
				SelectButton.Disabled = true;
				continue;
			}
			PrerequisitesLabel.Text = PrerequisitesLabel.Text.Replace($"[color=red]{prerequisite}[/color]", $"[color=green]{prerequisite}[/color]");
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Globals.SoulListeners.Remove(Update);
	}
}
