using Godot;
using System;
using Necromation;
using Necromation.gui;

public partial class TechPanel : PanelContainer
{
	private Label TitleLabel => GetNode<Label>("%TitleLabel");
	private Label CostLabel => GetNode<Label>("%CostLabel");
	private Label DescriptionLabel => GetNode<Label>("%DescriptionLabel");
	private Label UnlocksLabel => GetNode<Label>("%UnlocksLabel");
	private HBoxContainer EffectsRow => GetNode<HBoxContainer>("%EffectsRow");
	private TextureRect CostTexture => GetNode<TextureRect>("%CostTexture");
	private Button SelectButton => GetNode<Button>("%SelectButton");

	public Technology Tech;

	public override void _Ready()
	{
		base._Ready();
		
		if (Tech == null) return;
		
		TitleLabel.Text = Tech.Name;
		CostLabel.Text = Tech.Count.ToString();

		SelectButton.Pressed += () => Globals.CurrentTechnology = Tech;
	}
}
