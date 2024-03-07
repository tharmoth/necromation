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
			UpdateIcon();
		}
	}

	protected virtual void UpdateIcon()
	{
		if (_itemType == null)
		{
			Icon.Visible = false;
		}
		else
		{
			Icon.Visible = true;
			Icon.Texture = Database.Instance.GetTexture(_itemType);
		}
	}
}
