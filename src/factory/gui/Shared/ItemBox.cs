using Godot;
using System;

public partial class ItemBox : PanelContainer
{
	protected TextureRect Icon => GetNode<TextureRect>("%Icon");
	protected Label CountLabel => GetNode<Label>("%CountLabel");
	protected Button Button => GetNode<Button>("%Button");
	
	private string _itemType;
	public string ItemType
	{
		get => _itemType;
		set
		{
			_itemType = value;
			Icon.Texture = Database.Instance.GetTexture(_itemType);
		}
	}

	public int Count
	{
		get => int.Parse(CountLabel.Text);
		set => CountLabel.Text = value.ToString();
	}
}
