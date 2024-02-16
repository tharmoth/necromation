﻿using Godot;

namespace Necromation;

public partial class GroundItem : Node2D, BuildingTileMap.IEntity
{
    public string ItemType { get; set; }
    private Sprite2D _sprite;
	
    public GroundItem(string itemType)
    {
        ItemType = itemType;
        _sprite = new Sprite2D();
        _sprite.ZIndex = 1;
        _sprite.Scale = new Vector2(0.5f, 0.5f);
        AddChild(_sprite);
    }
    
    public override void _Ready()
    {
        base._Ready();
        _sprite.Texture = GD.Load<Texture2D>($"res://res/sprites/{ItemType}.png");
    }
    
    public bool AddToInventory(Inventory inventory)
    {
        inventory.Insert(ItemType);
        QueueFree();
        return true;
    }
}