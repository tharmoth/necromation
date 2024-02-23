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
	private HBoxContainer EffectsRow => GetNode<HBoxContainer>("%EffectsRow");
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
		CostLabel.Text = Tech.Count.ToString();
		DescriptionLabel.Text = Tech.Description;

		UnlocksLabel.Text = Tech.Unlocks.Count > 0
			? string.Join(", ", Tech.Unlocks)
			: "No unlocks";
		
		Tech.Unlocks.ToList().ForEach(unlock =>
		{
			GD.Print(unlock);
			var recipe = Globals.Database.Recipes.First(recipe => recipe.Name == unlock);
			var texture = new TextureRect
			{
				Texture = recipe.GetTexture(),
			};
			EffectsRow.AddChild(texture);
			CraftingListPopup.Register(recipe, texture);
		});

		PrerequisitesLabel.Text = Tech.Prerequisites.Count > 0
			? string.Join(", ", Tech.Prerequisites)
			: "No prerequisites";

		SelectButton.Pressed += () => Globals.CurrentTechnology = Tech;

		foreach (var prerequisite in Tech.Prerequisites)
		{
			var researched = Globals.Database.Technologies
				.Where(tech => tech.Researched)
				.Select(tech => tech.Name)
				.Any(name => name == prerequisite);
			if (!researched)
			{
				SelectButton.Disabled = true;
				continue;
			}
			PrerequisitesLabel.Text = PrerequisitesLabel.Text.Replace(prerequisite, $"[color=green]{prerequisite}[/color]");
		}
		
	}
}
