using Godot;

namespace Necromation;

public partial class GroundItem : Node2D, BuildingTileMap.IEntity
{
    public string ItemType { get; set; }
    private Sprite2D _sprite;
    private Vector2 _cachePosition;
    public Vector2 CachePosition
    {
        get => _cachePosition;
        set
        {
            _cachePosition = value;
            GlobalPosition = _cachePosition;
        }
    }

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
        _sprite.Texture = Globals.Database.GetTexture(ItemType);
        _sprite.Scale = new Vector2(16 / _sprite.Texture.GetSize().X, 16 / _sprite.Texture.GetSize().Y);
        CachePosition = GlobalPosition;
    }
    
    public bool AddToInventory(Inventory inventory)
    {
        inventory.Insert(ItemType);
        QueueFree();
        return true;
    }
}