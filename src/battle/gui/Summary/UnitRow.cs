using Godot;
using System;
using Necromation;

public partial class UnitRow : HBoxContainer
{
	private TextureRect Sprite => GetNode<TextureRect>("%Sprite");
	private Label NameLabel => GetNode<Label>("%Name");
	private Label CountLabel => GetNode<Label>("%Count");
	private Label KillsLabel => GetNode<Label>("%Kills");
	private Label DeathsLabel => GetNode<Label>("%Deaths");

	public void Init(UnitStats stat)
	{
		Sprite.Texture = Database.Instance.GetTexture(stat.UnitType);
		NameLabel.Text = stat.UnitType;
		CountLabel.Text = stat.Count.ToString();
		KillsLabel.Text = stat.Kills.ToString();
		DeathsLabel.Text = stat.Deaths.ToString();
	}
}
